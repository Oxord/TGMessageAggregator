import React, { useState } from 'react';
import EndpointCard from './EndpointCard';
import { callApi } from '../api';
import { Summary } from '../types/models'; // Import Summary type
import { AxiosRequestConfig } from 'axios';

const TelegramControllerComponent: React.FC = () => {
  // State for GET /chats and POST /chats/summary inputs
  const [chatId, setChatId] = useState<string>(''); // Input as string, backend expects long
  const [count, setCount] = useState<string>('100'); // Input as string, backend expects int

  // --- Handler for GET /api/telegram/chats ---
  const handleGetChatsSubmit = async (): Promise<string[]> => { // Expecting list of strings based on controller
    if (!chatId) {
        throw new Error('Chat ID cannot be empty.');
    }
    const params = new URLSearchParams();
    params.append('chatId', chatId);
    if (count) {
        params.append('count', count);
    }

    const config: AxiosRequestConfig = {
      method: 'GET',
      url: `/api/telegram/chats?${params.toString()}`, // Note: Controller route is just /chats
    };
    // Assuming the backend returns a list of strings for messages
    return callApi<string[]>(config);
  };

  // --- Handler for POST /api/telegram/chats/summary ---
  const handleSummarizeChatSubmit = async (): Promise<Summary> => {
    if (!chatId) {
        throw new Error('Chat ID cannot be empty.');
    }
    const params = new URLSearchParams();
    params.append('chatId', chatId);
    if (count) {
        params.append('count', count);
    }

    const config: AxiosRequestConfig = {
      method: 'POST',
      url: `/api/telegram/chats/summary?${params.toString()}`, // Note: Controller route is just /chats/summary
      // No request body needed for this POST, parameters are in query string
    };
    return callApi<Summary>(config);
  };

  return (
    <div>
      {/* Endpoint: GET /api/telegram/chats */}
      <EndpointCard
        method="GET"
        // Corrected path based on controller attribute [HttpGet("/chats")]
        path="/chats"
        summary="Gets messages from a specific Telegram chat."
        parameters={
          <>
            <div>
              <label htmlFor="getChatsChatId">chatId (long):</label>
              <input
                id="getChatsChatId"
                type="number" // Use number input for ID
                value={chatId}
                onChange={(e) => setChatId(e.target.value)}
                placeholder="Enter Telegram Chat ID"
                required
              />
            </div>
            <div>
              <label htmlFor="getChatsCount">count (int, optional):</label>
              <input
                id="getChatsCount"
                type="number"
                value={count}
                onChange={(e) => setCount(e.target.value)}
                placeholder="Default: 100"
              />
            </div>
          </>
        }
        onSubmit={handleGetChatsSubmit}
      />

      {/* Endpoint: POST /api/telegram/chats/summary */}
      <EndpointCard
        method="POST"
        // Corrected path based on controller attribute [HttpPost("/chats/summary")]
        path="/chats/summary"
        summary="Gets messages from a specific Telegram chat and generates a summary."
        parameters={
           <>
            <div>
              <label htmlFor="summarizeChatId">chatId (long):</label>
              <input
                id="summarizeChatId"
                type="number" // Use number input for ID
                value={chatId}
                onChange={(e) => setChatId(e.target.value)}
                placeholder="Enter Telegram Chat ID"
                required
              />
            </div>
            <div>
              <label htmlFor="summarizeCount">count (int, optional):</label>
              <input
                id="summarizeCount"
                type="number"
                value={count}
                onChange={(e) => setCount(e.target.value)}
                placeholder="Default: 100"
              />
            </div>
          </>
        }
        onSubmit={handleSummarizeChatSubmit}
      />
    </div>
  );
};

export default TelegramControllerComponent;
