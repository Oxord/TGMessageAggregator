import React, { useState } from 'react';
import EndpointCard from './EndpointCard';
import { callApi } from '../api';
import { Summary, AnalyzeRequestData } from '../types/models'; // Import necessary types
import { AxiosRequestConfig } from 'axios';

const DcaControllerComponent: React.FC = () => {
  // State for POST /analyze inputs
  const [analyzeChatName, setAnalyzeChatName] = useState<string>('');
  const [analyzeData, setAnalyzeData] = useState<string>(''); // Input as JSON string

  // State for GET /summaries/{categoryName} input
  const [categoryName, setCategoryName] = useState<string>('');

  // --- Handler for POST /api/dca/analyze ---
  const handleAnalyzeSubmit = async (): Promise<Summary> => {
    let requestBody: AnalyzeRequestData;
    try {
      // Assuming analyzeData is a JSON array of strings like '["msg1", "msg2"]'
      const dataArray = JSON.parse(analyzeData);
      if (!Array.isArray(dataArray) || !dataArray.every(item => typeof item === 'string')) {
        throw new Error('Data must be a valid JSON array of strings.');
      }
      requestBody = { Data: dataArray, ChatName: analyzeChatName };
    } catch (e) {
      throw new Error('Invalid JSON format for Data field. Please provide an array of strings (e.g., ["message1", "message2"]).');
    }

    const config: AxiosRequestConfig = {
      method: 'POST',
      url: '/api/dca/analyze',
      data: requestBody,
    };
    return callApi<Summary>(config);
  };

  // --- Handler for GET /api/dca/summaries ---
  const handleGetAllSummariesSubmit = async (): Promise<Summary[]> => {
    const config: AxiosRequestConfig = {
      method: 'GET',
      url: '/api/dca/summaries',
    };
    return callApi<Summary[]>(config);
  };

  // --- Handler for GET /api/dca/summaries/{categoryName} ---
  const handleGetSummariesByCategorySubmit = async (): Promise<Summary[]> => {
    if (!categoryName) {
        throw new Error('Category name cannot be empty.');
    }
    const config: AxiosRequestConfig = {
      method: 'GET',
      url: `/api/dca/summaries/${encodeURIComponent(categoryName)}`,
    };
    return callApi<Summary[]>(config);
  };


  return (
    <div>
      {/* Endpoint: POST /api/dca/analyze */}
      <EndpointCard
        method="POST"
        path="/api/dca/analyze"
        summary="Analyzes messages and returns a summary."
        requestBody={
          <>
            <label htmlFor="analyzeChatName">ChatName (string):</label>
            <input
              id="analyzeChatName"
              type="text"
              value={analyzeChatName}
              onChange={(e) => setAnalyzeChatName(e.target.value)}
              placeholder="Enter chat name"
              required
            />
            <label htmlFor="analyzeData">Data (JSON array of strings):</label>
            <textarea
              id="analyzeData"
              value={analyzeData}
              onChange={(e) => setAnalyzeData(e.target.value)}
              placeholder='e.g., ["message one", "message two"]'
              required
            />
          </>
        }
        onSubmit={handleAnalyzeSubmit}
      />

      {/* Endpoint: GET /api/dca/summaries */}
      <EndpointCard
        method="GET"
        path="/api/dca/summaries"
        summary="Retrieves all previously generated summaries."
        onSubmit={handleGetAllSummariesSubmit}
      />

      {/* Endpoint: GET /api/dca/summaries/{categoryName} */}
      <EndpointCard
        method="GET"
        path="/api/dca/summaries/{categoryName}"
        summary="Retrieves summaries filtered by category name."
        parameters={
          <>
            <label htmlFor="categoryName">categoryName (string):</label>
            <input
              id="categoryName"
              type="text"
              value={categoryName}
              onChange={(e) => setCategoryName(e.target.value)}
              placeholder="Enter category name"
              required
            />
          </>
        }
        onSubmit={handleGetSummariesByCategorySubmit}
      />
    </div>
  );
};

export default DcaControllerComponent;
