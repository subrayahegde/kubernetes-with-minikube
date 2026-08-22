import amqp from 'amqplib';
// Use the 'import type' syntax for TS interfaces/types so they don't leak into compiled JavaScript
import type { Connection, Channel, Message } from 'amqplib';

const RABBITMQ_URL: string = process.env.RABBITMQ_URL
const QUEUE_NAME: string = 'video_tasks';

// Type definition for the expected task payload
interface VideoTask {
  jobId: string | number;
  prompt: string;
}

// Helper to simulate a heavy processing delay
const delay = (ms: number): Promise<void> => 
  new Promise((resolve) => setTimeout(resolve, ms));

async function startWorker(): Promise<void> {
  try {
    const connection: Connection = await amqp.connect(RABBITMQ_URL);
    const channel: Channel = await connection.createChannel();

    await channel.assertQueue(QUEUE_NAME, { durable: true });
    
    // Fair dispatch: don't give more than 1 message to a worker at a time
    channel.prefetch(1); 

    console.log(`🚀 FlashForge Worker started. Waiting for jobs in "${QUEUE_NAME}"...`);

    await channel.consume(QUEUE_NAME, async (msg: Message | null) => {
      if (msg !== null) {
        try {
          const task: VideoTask = JSON.parse(msg.content.toString());
          console.log(`\n📦 [Job received] ID: ${task.jobId} for Prompt: "${task.prompt}"`);

          // --- Simulate Video Production Pipeline ---
          
          console.log(`   ⏳ [1/3] Generating voiceover audio via ElevenLabs API...`);
          await delay(3000); 

          console.log(`   ⏳ [2/3] Extracting keywords and fetching stock clips...`);
          await delay(3000);

          console.log(`   ⏳ [3/3] Compiling and stitching video using FFmpeg...`);
          await delay(4000);

          console.log(`✅ [Job Completed] Video is ready for Job ID: ${task.jobId}! Saved to S3 storage.`);

          // Acknowledge the message to remove it from RabbitMQ
          channel.ack(msg);
        } catch (parseError) {
          console.error('❌ Failed to process message content:', parseError);
          // Reject malformed messages so they don't block the queue permanently
          channel.nack(msg, false, false);
        }
      }
    });
  } catch (error) {
    console.error('Worker error:', error);
  }
}

startWorker();

