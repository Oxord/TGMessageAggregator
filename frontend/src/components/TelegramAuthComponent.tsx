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
      <h3 className="auth-title">Telegram Auth</h3>
      {!isCodeRequested ? (
        <div>
          <label htmlFor="phoneNumber">Телефон</label>
          <input
            id="phoneNumber"
            type="tel"
            value={phoneNumber}
            onChange={(e) => setPhoneNumber(e.target.value)}
            placeholder="+7XXXXXXXXXX"
            disabled={isLoading}
            required
          />
          <button onClick={handleRequestCode} disabled={isLoading || !phoneNumber}>
            {isLoading ? 'Отправка...' : 'Получить код'}
          </button>
        </div>
      ) : (
        <div>
          <label htmlFor="phoneNumberStatic">Телефон</label>
          <input id="phoneNumberStatic" type="text" value={phoneNumber} readOnly disabled />
          <label htmlFor="code">Код</label>
          <input
            id="code"
            type="text"
            value={code}
            onChange={(e) => setCode(e.target.value)}
            placeholder="Код"
            disabled={isLoading}
            required
          />
          <button onClick={handleSubmitCode} disabled={isLoading || !code}>
            {isLoading ? 'Проверка...' : 'Ввести код'}
          </button>
          <button onClick={() => { setIsCodeRequested(false); setMessage(''); setError(''); setCode(''); }} disabled={isLoading} style={{ marginLeft: '10px' }}>
            Изменить телефон
          </button>
        </div>
      )}

      {message && <p className="success-message">{message}</p>}
      {error && <p className="error-message">{error}</p>}
    </div>
  );
};

export default TelegramAuthComponent;
