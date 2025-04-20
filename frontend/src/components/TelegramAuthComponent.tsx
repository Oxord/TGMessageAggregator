import React, { useState } from 'react';
import { callApi } from '../api';
import { AxiosRequestConfig } from 'axios';

const TelegramAuthComponent: React.FC = () => {
  const [phoneNumber, setPhoneNumber] = useState<string>('');
  const [code, setCode] = useState<string>('');
  const [isCodeRequested, setIsCodeRequested] = useState<boolean>(false);
  const [message, setMessage] = useState<string>('');
  const [error, setError] = useState<string>('');
  const [isLoading, setIsLoading] = useState<boolean>(false);

  const handleRequestCode = async () => {
    if (!phoneNumber) {
      setError('Phone number cannot be empty.');
      return;
    }
    setIsLoading(true);
    setError('');
    setMessage('');

    const config: AxiosRequestConfig = {
      method: 'POST',
      url: '/api/telegram/request-code', // Matches backend route
      data: { phoneNumber },
    };

    try {
      const response = await callApi<{ message: string }>(config);
      setMessage(response.message || 'Verification code sent.');
      setIsCodeRequested(true);
    } catch (err: any) {
      setError(err.response?.data?.message || err.message || 'Failed to request code.');
      setIsCodeRequested(false); // Reset if request failed
    } finally {
      setIsLoading(false);
    }
  };

  const handleSubmitCode = async () => {
    if (!phoneNumber || !code) {
      setError('Phone number and code cannot be empty.');
      return;
    }
    setIsLoading(true);
    setError('');
    setMessage('');

    const config: AxiosRequestConfig = {
      method: 'POST',
      url: '/api/telegram/submit-code', // Matches backend route
      data: { phoneNumber, code },
    };

    try {
      const response = await callApi<{ message: string }>(config);
      setMessage(response.message || 'Telegram account linked successfully.');
      // Optionally reset state after success
      // setPhoneNumber('');
      // setCode('');
      // setIsCodeRequested(false);
    } catch (err: any) {
      setError(err.response?.data?.message || err.response?.data || err.message || 'Failed to submit code.');
       // Keep code input visible on error to allow retry
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="endpoint-card">
      <h3>Telegram Authentication</h3>
      {!isCodeRequested ? (
        // Step 1: Enter Phone Number
        <div>
          <p>Enter your phone number to link your Telegram account.</p>
          <div>
            <label htmlFor="phoneNumber">Phone Number:</label>
            <input
              id="phoneNumber"
              type="tel" // Use 'tel' type for phone numbers
              value={phoneNumber}
              onChange={(e) => setPhoneNumber(e.target.value)}
              placeholder="e.g., +1234567890"
              disabled={isLoading}
              required
            />
          </div>
          <button onClick={handleRequestCode} disabled={isLoading || !phoneNumber}>
            {isLoading ? 'Sending...' : 'Request Code'}
          </button>
        </div>
      ) : (
        // Step 2: Enter Code
        <div>
          <p>Enter the code sent to your Telegram account for {phoneNumber}.</p>
           {/* Display phone number again for confirmation */}
           <div>
             <label htmlFor="phoneNumberStatic">Phone Number:</label>
             <input id="phoneNumberStatic" type="text" value={phoneNumber} readOnly disabled />
           </div>
          <div>
            <label htmlFor="code">Verification Code:</label>
            <input
              id="code"
              type="text" // Code might contain non-numeric characters
              value={code}
              onChange={(e) => setCode(e.target.value)}
              placeholder="Enter code"
              disabled={isLoading}
              required
            />
          </div>
          <button onClick={handleSubmitCode} disabled={isLoading || !code}>
            {isLoading ? 'Verifying...' : 'Submit Code'}
          </button>
          <button onClick={() => { setIsCodeRequested(false); setMessage(''); setError(''); setCode(''); /* Keep phone number */ }} disabled={isLoading} style={{ marginLeft: '10px' }}>
            Change Phone Number
          </button>
        </div>
      )}

      {/* Display Messages/Errors */}
      {message && <p className="success-message">{message}</p>}
      {error && <p className="error-message">{error}</p>}
    </div>
  );
};

export default TelegramAuthComponent;
