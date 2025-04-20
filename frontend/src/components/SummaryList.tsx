import React from 'react';
import { Summary } from '../types/models';

interface SummaryListProps {
  summaries: Summary[];
}

const SummaryList: React.FC<SummaryListProps> = ({ summaries }) => {
  if (!summaries.length) {
    return <div>Нет данных для отображения.</div>;
  }

  return (
    <ul style={{ paddingLeft: 0, listStyle: 'none' }}>
      {summaries.map((summary) => (
        <li
          key={summary.id}
          style={{
            border: '1px solid #ddd',
            borderRadius: 8,
            marginBottom: 16,
            padding: 12,
            background: '#fafbfc',
          }}
        >
          <div>
            <strong>Чат:</strong> {summary.chatName ?? <em>нет</em>}
          </div>
          <div>
            <strong>Описание:</strong> {summary.description}
          </div>
          <div>
            <strong>Категория:</strong> {summary.categoryName}
          </div>
          <div>
            <strong>Дата создания:</strong> {new Date(summary.createdAt).toLocaleString()}
          </div>
          <div style={{ fontSize: 12, color: '#888', marginTop: 4 }}>
            <strong>ID:</strong> {summary.id}
          </div>
        </li>
      ))}
    </ul>
  );
};

export default SummaryList;
