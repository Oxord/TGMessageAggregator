import React from 'react';
import './App.css';
// Import the created components
import DcaControllerComponent from './components/DcaControllerComponent';
import TelegramControllerComponent from './components/TelegramControllerComponent';
import TelegramAuthComponent from './components/TelegramAuthComponent'; // Import the new auth component

function App() {
  return (
    <div className="app-container">
      <h1>API Interface</h1>

      {/* Placeholder for DcaController endpoints */}
      <div className="controller-section">
        <h2>Dca Controller (/api/dca)</h2>
        <DcaControllerComponent />
      </div>

      {/* Use TelegramControllerComponent */}
      <div className="controller-section">
        {/* Corrected path based on controller attribute [Route("/api/telegram")] */}
        <h2>Telegram Controller (/api/telegram)</h2>
        {/* Add the Authentication Component */}
        <TelegramAuthComponent />
        {/* Keep the existing component for chat/summary actions */}
        <TelegramControllerComponent />
      </div>
    </div>
  );
}

export default App;
