import React from 'react';
import './App.css';
// Import the created components
import DcaControllerComponent from './components/DcaControllerComponent';
import TelegramControllerComponent from './components/TelegramControllerComponent';
import TelegramAuthComponent from './components/TelegramAuthComponent'; // Import the new auth component

function App() {
  return (
    <div className="app-container">
      <h1 className="project-title">TGMessageAggregator</h1>

      <div className="controller-section">
        <DcaControllerComponent />
      </div>

      <div className="controller-section">
        <TelegramAuthComponent />
        <TelegramControllerComponent />
      </div>
    </div>
  );
}

export default App;
