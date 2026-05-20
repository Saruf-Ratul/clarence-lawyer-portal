import React from 'react';
import { createRoot } from 'react-dom/client';

function App() {
  return (
    <main style={{ fontFamily: 'system-ui', padding: 24 }}>
      <h1>Clarence Lawyer Portal</h1>
      <p>MVP scaffold initialized. Next: dashboard, intake inbox, and case workflows.</p>
    </main>
  );
}

createRoot(document.getElementById('root')!).render(<App />);
