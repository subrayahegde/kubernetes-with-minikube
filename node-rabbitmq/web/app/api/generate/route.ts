import { NextResponse } from 'next/server';
import amqp from 'amqplib';

const RABBITMQ_URL = process.env.RABBITMQ_URL || "amqp://admin:supersecret@rabbitmq:5672";

const QUEUE_NAME = 'video_tasks';

export async function POST(request: Request) {
  try {
    const { userId, prompt } = await request.json();

    if (!prompt) {
      return NextResponse.json({ error: 'Prompt is required' }, { status: 400 });
    }

    // 1. Connect to RabbitMQ
    const connection = await amqp.connect(RABBITMQ_URL);
    const channel = await connection.createChannel();
    
    // 2. Ensure queue exists
    await channel.assertQueue(QUEUE_NAME, { durable: true });

    // 3. Create payload data
    const jobId = Math.random().toString(36).substring(7);
    const payload = { jobId, userId, prompt, timestamp: new Date() };

    // 4. Send message to queue
    channel.sendToQueue(QUEUE_NAME, Buffer.from(JSON.stringify(payload)), {
      persistent: true, // Persists message to disk so it survives broker crashes
    });

    // 5. Clean up connection
    await channel.close();
    await connection.close();

    return NextResponse.json({ success: true, jobId, message: 'Task queued successfully!' });
  } catch (error) {
    console.error('RabbitMQ Error:', error);
    return NextResponse.json({ error: 'Failed to queue task' }, { status: 500 });
  }
}
