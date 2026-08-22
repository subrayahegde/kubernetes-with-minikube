'use client'

import { useState } from 'react';

export default function Home() {
  const [prompt, setPrompt] = useState('');
  const [status, setStatus] = useState('');
  const [jobId, setJobId] = useState('');

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setStatus('Submitting...');

    try {
      const res = await fetch('/api/generate', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ userId: 'user_101', prompt }),
      });
      const data = await res.json();
      
      if (data.success) {
        setJobId(data.jobId);
        setStatus('Accepted! Your video processing request is sitting in RabbitMQ.');
      } else {
        setStatus(`Error: ${data.error}`);
      }
    } catch (err) {
      setStatus('Failed to connect to API.');
    }
  };

  return (
    <main className="p-8 max-w-xl mx-auto space-y-6">
      <h1 className="text-3xl font-bold">🎬 FlashForge Video Generator</h1>
      <p className="text-gray-600">The Next.js app sends requests instantly. RabbitMQ processes them safely in the background.</p>
      
      <form onSubmit={handleSubmit} className="space-y-4">
        <textarea
          className="w-full p-3 border border-gray-300 rounded shadow-sm text-black"
          rows={4}
          placeholder="Enter script or prompt for video..."
          value={prompt}
          onChange={(e) => setPrompt(e.target.value)}
          required
        />
        <button type="submit" className="w-full bg-blue-600 text-white py-2 rounded hover:bg-blue-700 transition">
          Generate Video
        </button>
      </form>

      {status && (
        <div className="p-4 bg-gray-100 rounded border border-gray-200 text-black">
          <p className="font-semibold">Status Update:</p>
          <p className="text-sm text-gray-700 mt-1">{status}</p>
          {jobId && <p className="text-xs text-blue-600 mt-2 font-mono">Job ID: {jobId}</p>}
        </div>
      )}
    </main>
  );
}

