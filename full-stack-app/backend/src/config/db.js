import pg from 'pg';

const { Pool } = pg;

const pool = new Pool({
  connectionString: process.env.DATABASE_URL || "postgresql://postgres:Password123@localhost:5432/clidb?sslmode=disable"
});

async function testConnection() {
  let client; // Scope declaration outside the try block
  try {
    // Establish connection and assign to the scoped variable
    client = await pool.connect();
    console.log('✅ Connection to PostgreSQL has been established successfully.');

    // Execute test query
    const res = await client.query('SELECT NOW()');
    console.log('🕒 Server Time:', res.rows[0].now);
  } catch (err) {
    console.error('❌ Connection error:', err.stack);
  } finally {
    // Release the client safely if it was successfully created
    if (client) {
      client.release();
    }
  }
}

testConnection();

export const query = (text, params) => pool.query(text, params);
