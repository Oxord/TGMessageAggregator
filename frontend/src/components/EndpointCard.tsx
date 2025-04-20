import React, { useState, ReactNode } from 'react';
import axios, { AxiosRequestConfig, Method } from 'axios';
import SummaryList from './SummaryList';
import { Summary } from '../types/models';

interface EndpointCardProps {
  method: Method;
  path: string;
  summary: string;
  parameters?: ReactNode; // For query/path params inputs
  requestBody?: ReactNode; // For body input
  onSubmit: (config: AxiosRequestConfig) => Promise<any>; // Function to call API
}

const EndpointCard: React.FC<EndpointCardProps> = ({
  method,
  path,
  summary,
  parameters,
  requestBody,
  onSubmit,
}) => {
  const [isOpen, setIsOpen] = useState(false);
  const [response, setResponse] = useState<any>(null);
  const [error, setError] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(false);

  const handleToggle = () => setIsOpen(!isOpen);

  const handleSubmit = async (event: React.FormEvent) => {
    event.preventDefault();
    setIsLoading(true);
    setError(null);
    setResponse(null);

    // The actual Axios config will be built by the parent component
    // based on the inputs provided via 'parameters' and 'requestBody' props.
    // For now, we just call the passed onSubmit function.
    // We need a way to pass the input data from parameters/requestBody
    // back to the parent to construct the actual request config.
    // This will be refined when implementing the specific controller components.

    // Placeholder for actual request logic - this needs refinement
    try {
        // This is conceptual - the parent component will need to provide
        // the logic to gather data from the form elements passed as children
        // and construct the appropriate AxiosRequestConfig.
        // For now, we'll pass a basic config.
        const config: AxiosRequestConfig = {
            method: method,
            url: path, // Base URL needs to be configured
        };
        const result = await onSubmit(config); // Parent handles actual call
        setResponse(result);
    } catch (err: any) {
        if (axios.isAxiosError(err)) {
            setError(`Error: ${err.response?.status} ${err.response?.statusText} - ${JSON.stringify(err.response?.data)}`);
        } else {
            setError(`An unexpected error occurred: ${err.message}`);
        }
    } finally {
        setIsLoading(false);
    }
  };

  const getMethodClass = (method: Method): string => {
    switch (method.toUpperCase()) {
      case 'POST': return 'method-post';
      case 'GET': return 'method-get';
      case 'PUT': return 'method-put';
      case 'DELETE': return 'method-delete';
      default: return '';
    }
  };

  return (
    <div className="endpoint-card">
      <div className="endpoint-header" onClick={handleToggle}>
        <div>
          <span className={`endpoint-method method-blue`}>{method}</span>
          <span className="endpoint-path">{path}</span>
        </div>
        <span>{isOpen ? '▲' : '▼'}</span>
      </div>
      {isOpen && (
        <div className="endpoint-details">
          <form onSubmit={handleSubmit}>
            {parameters && (
              <div className="parameters">
                {parameters}
              </div>
            )}
            {requestBody && (
              <div className="request-body">
                {requestBody}
              </div>
            )}
            <button type="submit" disabled={isLoading}>
              {isLoading ? 'Отправка...' : 'Выполнить'}
            </button>
          </form>

          {(response || error || isLoading) && (
            <div className="response">
              {isLoading && <p>Загрузка...</p>}
              {error && <pre className="error">{error}</pre>}
              {response && (
                Array.isArray(response) && response.length > 0 && isSummaryArray(response) ? (
                  <SummaryList summaries={response as Summary[]} />
                ) : isSummaryObject(response) ? (
                  <SummaryList summaries={[response as Summary]} />
                ) : (
                  <pre>{JSON.stringify(response, null, 2)}</pre>
                )
              )}
            </div>
          )}
        </div>
      )}
    </div>
  );
};

function isSummaryObject(obj: any): obj is Summary {
  return obj &&
    typeof obj === 'object' &&
    typeof obj.id === 'string' &&
    'description' in obj &&
    'categoryName' in obj &&
    'createdAt' in obj;
}

function isSummaryArray(arr: any[]): arr is Summary[] {
  return arr.length > 0 && arr.every(isSummaryObject);
}

export default EndpointCard;
