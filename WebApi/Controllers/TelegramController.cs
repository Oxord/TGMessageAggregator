using System.ComponentModel.DataAnnotations;
using System.Security.Claims; // Added for User ID
using MessageAggregator.Application.Service;
using MessageAggregator.Domain.DTOs;
using MessageAggregator.Domain.Interfaces;
using MessageAggregator.Domain.Models;
using Microsoft.AspNetCore.Authorization; // Added for Authorize attribute
using Microsoft.AspNetCore.Http; // Added for HttpContext.Items
using Microsoft.AspNetCore.Identity; // Added for UserManager
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options; // Added for IOptions
using Microsoft.Extensions.Logging; // Added for ILogger
using System; // Added for Guid, TimeSpan
using System.IO; // Added for Path, File
using System.Text; // Added for Encoding
using System.Text.RegularExpressions; // For FLOOD_WAIT_X handling
using TL;
using WTelegram; // Added for WTelegram.Client

namespace WebApi.Controllers;

// DTOs for API requests
public record RequestCodeDto([Required] string PhoneNumber);
public record SubmitCodeDto([Required] string PhoneNumber, [Required] string Code);

[ApiController]
[Route("api/[controller]")] // Changed route template for consistency
public class TelegramController : ControllerBase
{
    // Fully qualify Identity User to avoid ambiguity with TL.User
    private readonly UserManager<MessageAggregator.Domain.Models.User> _userManager;
    private readonly TelegramSettings _telegramSettings;
    private readonly IDcaService _dcaService; // Kept for SummarizeChat (needs rework later)
    private readonly ILogger<TelegramController> _logger; // Added logger
    // Removed TelegramService injection

    // Constants for Session keys
    private const string TelegramLoginAttemptIdKey = "TelegramLoginAttemptId";

    public TelegramController(
        UserManager<MessageAggregator.Domain.Models.User> userManager, // Fully qualified
        IOptions<TelegramSettings> telegramOptions,
        IDcaService dcaService, // Keep for potential future use
        ILogger<TelegramController> logger) // Inject logger
    {
        _userManager = userManager;
        _telegramSettings = telegramOptions.Value;
        _dcaService = dcaService;
        _logger = logger; // Assign logger
    }

    // --- Telegram Authentication Endpoints (Refactored to use LoginUserIfNeeded) ---

    [HttpPost("request-code")]
    public async Task<IActionResult> RequestCode([FromBody] RequestCodeDto dto)
    {
        _logger.LogInformation("Attempting RequestCode v2 for PhoneNumber: {PhoneNumber}", dto?.PhoneNumber ?? "NULL");

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("RequestCode v2 ModelState invalid: {@ModelState}", ModelState);
            return BadRequest(ModelState);
        }

        // Sanitize phone number (remove spaces, ensure '+' prefix if needed - WTelegram might handle this)
        var sanitizedPhoneNumber = dto.PhoneNumber.Replace(" ", "");
        // Consider adding more robust validation/normalization if needed

        _logger.LogInformation("RequestCode v2 ModelState valid for SanitizedPhoneNumber: {PhoneNumber}", sanitizedPhoneNumber);

        // Generate a unique ID for this login attempt and store it in session
        var loginAttemptId = Guid.NewGuid().ToString("N");
        HttpContext.Session.SetString(TelegramLoginAttemptIdKey, loginAttemptId);
        _logger.LogDebug("Generated LoginAttemptId: {LoginAttemptId}", loginAttemptId);

        // Не сохраняем userId, request-code теперь доступен для всех (анонимно)

        var tempSessionPath = Path.Combine(Path.GetTempPath(), $"tg_session_{loginAttemptId}.session");
        _logger.LogDebug("Using temporary session path: {TempSessionPath}", tempSessionPath);

        // Config provider for the initial code request phase
        string? ConfigProvider(string what) => what switch
        {
            "api_id" => _telegramSettings.AppId,
            "api_hash" => _telegramSettings.ApiHash,
            "phone_number" => sanitizedPhoneNumber,
            "session_pathname" => tempSessionPath, // Use attempt-specific session file
            // Throw exception immediately when asked for code to prevent hang
            "verification_code" => throw new WTException("Verification code required."),
            _ => null // Let WTelegram handle defaults (like server address)
        };

        using var client = new Client(ConfigProvider);
        try
        {
            // This call will connect and request the code.
            // It's expected to fail because verification_code is null.
            await client.LoginUserIfNeeded();

            // If it somehow succeeds without asking for code (e.g., already authorized session),
            // it's an unexpected state for this endpoint.
            _logger.LogWarning("LoginUserIfNeeded succeeded unexpectedly during RequestCode for {PhoneNumber}", sanitizedPhoneNumber);
            // Clean up session state as this attempt is now invalid for SubmitCode
            HttpContext.Session.Remove(TelegramLoginAttemptIdKey);
            // Try deleting file after client disposal (handled by 'using')
            try { if (System.IO.File.Exists(tempSessionPath)) System.IO.File.Delete(tempSessionPath); } catch (Exception delEx) { _logger.LogWarning(delEx, "Failed to delete temp session file on unexpected success path: {TempSessionPath}", tempSessionPath); }
            return BadRequest("Unexpected state: Login completed without requesting code.");
        }
         // --- Catch specific Telegram RPC errors first ---
        catch (RpcException ex) when (ex.Code == 420 && ex.Message.StartsWith("FLOOD_WAIT_"))
        {
            // Parse wait seconds from message
            var match = Regex.Match(ex.Message, @"FLOOD_WAIT_(\d+)");
            int waitSeconds = match.Success ? int.Parse(match.Groups[1].Value) : -1;
            _logger.LogWarning("Telegram FLOOD_WAIT: must wait {WaitSeconds} seconds before retrying", waitSeconds);
            HttpContext.Session.Remove(TelegramLoginAttemptIdKey);
            if (waitSeconds > 0)
            {
                var waitMinutes = waitSeconds / 60;
                var waitHours = waitMinutes / 60;
                var waitMsg = waitHours > 0
                    ? $"Слишком частые попытки. Попробуйте снова через {waitHours} ч. {waitMinutes % 60} мин."
                    : $"Слишком частые попытки. Попробуйте снова через {waitMinutes} мин.";
                return StatusCode(429, waitMsg);
            }
            else
            {
                return StatusCode(429, "Слишком частые попытки. Попробуйте позже.");
            }
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "Telegram RpcException during RequestCode v2 for {PhoneNumber}: {ErrorCode} - {ErrorMessage}", sanitizedPhoneNumber, ex.Code, ex.Message);
            HttpContext.Session.Remove(TelegramLoginAttemptIdKey); // Clean up session state
            // Do not delete temp file here, SubmitCode's finally block handles it
            return BadRequest($"Telegram error: {ex.Message} ({ex.Code})");
        }
        // --- Catch other WTExceptions: Check if it's the expected "code required" error ---
        catch (WTException ex)
        {
            // Check if this is the specific exception we expect when code is needed
            if (ex.Message.Contains("Verification code required."))
            {
                // This is the EXPECTED exception. Log and return OK.
                _logger.LogInformation("LoginUserIfNeeded correctly requested verification code for {PhoneNumber}. LoginAttemptId: {LoginAttemptId}", sanitizedPhoneNumber, loginAttemptId);
                // The temporary session file (tempSessionPath) is left for SubmitCode.
                // Client is disposed automatically by 'using' block ending here.
                return Ok(new { Message = "Verification code sent." });
            }
            else
            {
                // It's a different WTException, log it as a general error and return 500.
                 _logger.LogError(ex, "Unexpected WTException during RequestCode v2 for {PhoneNumber}", sanitizedPhoneNumber);
                 HttpContext.Session.Remove(TelegramLoginAttemptIdKey); // Clean up session state
                 // Do not delete temp file here, SubmitCode's finally block handles it
                 return StatusCode(500, $"Internal Telegram library error: {ex.Message}");
            }
        }
         // --- Catch other general errors ---
        catch (Exception ex)
        {
            _logger.LogError(ex, "General Exception during RequestCode v2 for {PhoneNumber}", sanitizedPhoneNumber);
            HttpContext.Session.Remove(TelegramLoginAttemptIdKey); // Clean up session state
            // Do not delete temp file here, SubmitCode's finally block handles it
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
        // Note: 'using var client' ensures disposal even if exceptions occur.
        // The temporary session file persists ONLY if the expected WTException occurs.
    }


    [HttpPost("submit-code")]
    public async Task<IActionResult> SubmitCode([FromBody] SubmitCodeDto dto)
    {
        _logger.LogInformation("Attempting SubmitCode v2 for PhoneNumber: {PhoneNumber}", dto?.PhoneNumber ?? "NULL");

        if (!ModelState.IsValid)
        {
             _logger.LogWarning("SubmitCode v2 ModelState invalid: {@ModelState}", ModelState);
             return BadRequest(ModelState);
        }
        _logger.LogInformation("SubmitCode v2: ModelState valid."); // Added log

        // Retrieve the login attempt ID from the session
        var loginAttemptId = HttpContext.Session.GetString(TelegramLoginAttemptIdKey);
        if (string.IsNullOrEmpty(loginAttemptId))
        {
            _logger.LogWarning("SubmitCode v2: LoginAttemptId not found in session. Expired or invalid request sequence.");
            return BadRequest("Verification code request expired or was not initiated. Please request a new code.");
        }
        _logger.LogDebug("Retrieved LoginAttemptId: {LoginAttemptId}", loginAttemptId);

        // Sanitize phone number
        var sanitizedPhoneNumber = dto.PhoneNumber.Replace(" ", "");

        var tempSessionPath = Path.Combine(Path.GetTempPath(), $"tg_session_{loginAttemptId}.session");
        _logger.LogDebug("Using temporary session path: {TempSessionPath}", tempSessionPath);

        if (!System.IO.File.Exists(tempSessionPath))
        {
             _logger.LogError("SubmitCode v2: Temporary session file not found: {TempSessionPath}. Invalid request sequence.", tempSessionPath);
             HttpContext.Session.Remove(TelegramLoginAttemptIdKey); // Clean up session key
             return BadRequest("Verification code request expired or invalid. Please request a new code.");
        }

        // Config provider for the code submission phase
        string? ConfigProvider(string what) => what switch
        {
            "api_id" => _telegramSettings.AppId,
            "api_hash" => _telegramSettings.ApiHash,
            "phone_number" => sanitizedPhoneNumber,
            "session_pathname" => tempSessionPath, // Use the SAME session file from RequestCode
            "verification_code" => dto.Code, // Provide the code this time
            _ => null
        };

        string? sessionString = null;
        TL.User? telegramUser = null;

        Exception? loginException = null;
        // Step 1: Run client and login inside using block
        using (var client = new Client(ConfigProvider))
        {
            try
            {
                _logger.LogInformation("SubmitCode v2: Attempting client.LoginUserIfNeeded() for {PhoneNumber}", sanitizedPhoneNumber);
                await client.LoginUserIfNeeded();

                if (client.User == null)
                {
                    _logger.LogError("SubmitCode v2: LoginUserIfNeeded succeeded but client.User is null for {PhoneNumber}", sanitizedPhoneNumber);
                    return StatusCode(500, "Telegram login succeeded but failed to retrieve user details.");
                }

                _logger.LogInformation("LoginUserIfNeeded successful for {PhoneNumber}. User ID: {TelegramUserId}", sanitizedPhoneNumber, client.User.id);

                // Save user for later use (after using block)
                telegramUser = client.User;
            }
            catch (RpcException ex) when (ex.Code == 400 && ex.Message.Contains("PHONE_CODE_INVALID"))
            {
                _logger.LogWarning("SubmitCode v2: Invalid verification code provided for {PhoneNumber}. LoginAttemptId: {LoginAttemptId}", sanitizedPhoneNumber, loginAttemptId);
                return BadRequest("Invalid verification code.");
            }
            catch (RpcException ex)
            {
                _logger.LogError(ex, "Telegram RpcException during SubmitCode v2 for {PhoneNumber}: {ErrorCode} - {ErrorMessage}", sanitizedPhoneNumber, ex.Code, ex.Message);
                return BadRequest($"Telegram error: {ex.Message} ({ex.Code})");
            }
            catch (Exception ex)
            {
                loginException = ex;
            }
        }

        // Step 2: After client is disposed, read session file and update user
        if (loginException != null)
        {
            _logger.LogError(loginException, "General Exception during SubmitCode v2 for {PhoneNumber}", sanitizedPhoneNumber);
            return StatusCode(500, $"Internal server error: {loginException.Message}");
        }

        try
        {
            _logger.LogInformation("SubmitCode v2: Attempting to read session string from {TempSessionPath}", tempSessionPath);
            sessionString = await System.IO.File.ReadAllTextAsync(tempSessionPath, Encoding.UTF8);

            if (string.IsNullOrEmpty(sessionString))
            {
                _logger.LogError("Failed to read Telegram session string from temporary file {TempSessionPath} after successful login.", tempSessionPath);
                return StatusCode(500, "Failed to capture Telegram session after login.");
            }
            _logger.LogDebug("Successfully read session string from {TempSessionPath}", tempSessionPath);

            // На этом этапе можно вернуть Telegram user info и sessionString (или только user info)
            // Дальнейшая логика (создание/поиск пользователя) реализуется отдельно
            return Ok(new
            {
                TelegramUserId = telegramUser?.id,
                Username = telegramUser?.username,
                FirstName = telegramUser?.first_name,
                LastName = telegramUser?.last_name,
                Phone = telegramUser?.phone,
                SessionString = sessionString
            });
        }
        finally
        {
            var finalAttemptId = HttpContext.Session.GetString(TelegramLoginAttemptIdKey);
            if (System.IO.File.Exists(tempSessionPath))
            {
                try
                {
                    System.IO.File.Delete(tempSessionPath);
                    _logger.LogDebug("Deleted temporary session file: {TempSessionPath}", tempSessionPath);
                }
                catch (Exception delEx)
                {
                    _logger.LogWarning(delEx, "Failed to delete temporary session file: {TempSessionPath}", tempSessionPath);
                }
            }
            if (!string.IsNullOrEmpty(finalAttemptId))
            {
                HttpContext.Session.Remove(TelegramLoginAttemptIdKey);
            }
            // Удалять userId из сессии больше не требуется
        }
    }


    [HttpGet("status")]
    public async Task<IActionResult> GetStatus()
    {
        // TODO: Получение userId теперь должно быть реализовано через Telegram-авторизацию
        var userId = ""; // <-- Поставьте корректное получение userId через Telegram

        var currentUser = await _userManager.FindByIdAsync(userId);
        if (currentUser == null) return NotFound("User not found.");

        return Ok(new { IsLinked = !string.IsNullOrEmpty(currentUser.TelegramSessionString) });
    }

    [HttpDelete("unlink")]
    public async Task<IActionResult> Unlink()
    {
        // TODO: Получение userId теперь должно быть реализовано через Telegram-авторизацию
        var userId = ""; // <-- Поставьте корректное получение userId через Telegram

        var currentUser = await _userManager.FindByIdAsync(userId);
        if (currentUser == null) return NotFound("User not found.");

        currentUser.TelegramSessionString = null; // Clear the session string
        var updateResult = await _userManager.UpdateAsync(currentUser);

        if (updateResult.Succeeded)
        {
            return Ok(new { Message = "Telegram account unlinked successfully." });
        }
        else
        {
            return BadRequest(updateResult.Errors);
        }
    }


    // --- Existing Endpoints (Require Refactoring of TelegramService) ---
    /*
    [HttpGet("/chats")] // TODO: Refactor to use authenticated user's session via TelegramService
    public async Task<ActionResult<List<ChatBase>>> GetAllChats(
        [Required] [FromQuery] long chatId,
        [FromQuery] int count = 100
    )
    {
        // List<string> messages = await telegramService.GetMessagesAsync(chatId, count);
        // return Ok(messages);
        return Ok("Endpoint needs refactoring for user-specific sessions.");
    }

    [HttpPost("/chats/summary")] // TODO: Refactor to use authenticated user's session via TelegramService
    public async Task<ActionResult<Summary>> SummarizeChat(
        [Required] [FromQuery] long chatId,
        [FromQuery] int count = 100
    )
    {
        // List<string> messages = await telegramService.GetMessagesAsync(chatId, count);
        // Summary summary = await _dcaService.AnalyzeAndSummarizeAsync(messages, chatId.ToString());
        // return Ok(summary);
        return Ok("Endpoint needs refactoring for user-specific sessions.");
    }
    */
}
