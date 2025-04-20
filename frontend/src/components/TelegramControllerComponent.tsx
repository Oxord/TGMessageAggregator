import React, { useState } from 'react';
import EndpointCard from './EndpointCard';
import { callApi } from '../api';
import { Summary } from '../types/models'; // Import Summary type
import { AxiosRequestConfig } from 'axios';

const TelegramControllerComponent: React.FC = () => {
  // State for GET /chats and POST /chats/summary inputs
  const [chatId, setChatId] = useState<string>(''); // Input as string, backend expects long
  const [count, setCount] = useState<string>('100'); // Input as string, backend expects int
  const [verificationCode, setVerificationCode] = useState<string>(''); // State for verification code

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
    if (verificationCode) {
      params.append('verificationCode', verificationCode);
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
    if (verificationCode) {
      params.append('verificationCode', verificationCode);
    }

    const config: AxiosRequestConfig = {
      method: 'POST',
      url: `/api/telegram/chats/summary?${params.toString()}`, // Note: Controller route is just /chats/summary
    };
    return callApi<Summary>(config);
  };


  return (
    <div>
      {/* Endpoint: GET /api/telegram/chats */}
      <EndpointCard
        method="GET"
        path="/chats"
        summary=""
        parameters={
          <>
            <label htmlFor="getChatsChatId">Chat ID</label>
            <input
              id="getChatsChatId"
              type="number"
              value={chatId}
              onChange={(e) => setChatId(e.target.value)}
              placeholder="ID чата"
              required
            />
            <label htmlFor="getChatsCount">Count</label>
            <input
              id="getChatsCount"
              type="number"
              value={count}
              onChange={(e) => setCount(e.target.value)}
              placeholder="100"
            />
            <label htmlFor="getChatsVerificationCode">2FA Code</label>
            <input
              id="getChatsVerificationCode"
              type="text"
              value={verificationCode}
              onChange={(e) => setVerificationCode(e.target.value)}
              placeholder="Код"
            />
          </>
        }
        onSubmit={handleGetChatsSubmit}
      />

      <EndpointCard
        method="POST"
        path="/chats/summary"
        summary=""
        parameters={
          <>
            <label htmlFor="summarizeChatId">Chat ID</label>
            <input
              id="summarizeChatId"
              type="number"
              value={chatId}
              onChange={(e) => setChatId(e.target.value)}
              placeholder="ID чата"
              required
            />
            <label htmlFor="summarizeCount">Count</label>
            <input
              id="summarizeCount"
              type="number"
              value={count}
              onChange={(e) => setCount(e.target.value)}
              placeholder="100"
            />
            <label htmlFor="summarizeVerificationCode">2FA Code</label>
            <input
              id="summarizeVerificationCode"
              type="text"
              value={verificationCode}
              onChange={(e) => setVerificationCode(e.target.value)}
              placeholder="Код"
            />
          </>
        }
        onSubmit={handleSummarizeChatSubmit}
      />
    </div>
  );
};

export default TelegramControllerComponent;
