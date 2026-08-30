'use client';
import { useEffect, useState } from 'react';

interface Product {
  id?: number;
  name: string;
  description: string;
  price: number;
}

export default function Home() {
  const [products, setProducts] = useState<Product[]>([]);
  const [name, setName] = useState('');
  const [description, setDescription] = useState('');
  const [price, setPrice] = useState('');

  const API_URL = process.env.NEXT_PUBLIC_BACKEND_URL;

  const loadProducts = async () => {
    try {
      const res = await fetch(`${API_URL}/api/products`);
      const data = await res.json();
      setProducts(data);
    } catch (err) {
      console.error("Failed to fetch products:", err);
    }
  };

  useEffect(() => { loadProducts(); }, []);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!name || !price) return;
    await fetch(`${API_URL}/api/products`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ name, description, price: parseFloat(price) }),
    });
    setName(''); setDescription(''); setPrice('');
    loadProducts();
  };

  const handleDelete = async (id: number) => {
    await fetch(`${API_URL}/api/products/${id}`, { method: 'DELETE' });
    loadProducts();
  };

  return (
    <main style={{ padding: '2rem', fontFamily: 'sans-serif', maxWidth: '600px', margin: '0 auto' }}>
      <h1>Product Catalog</h1>
      
      <form onSubmit={handleSubmit} style={{ display: 'flex', flexDirection: 'column', gap: '10px', marginBottom: '2rem' }}>
        <input placeholder="Product Name" value={name} onChange={e => setName(e.target.value)} required style={{ padding: '8px' }} />
        <input placeholder="Description" value={description} onChange={e => setDescription(e.target.value)} style={{ padding: '8px' }} />
        <input placeholder="Price" type="number" step="0.01" value={price} onChange={e => setPrice(e.target.value)} required style={{ padding: '8px' }} />
        <button type="submit" style={{ padding: '10px', background: '#0070f3', color: 'white', border: 'none', cursor: 'pointer' }}>Add Product</button>
      </form>

      <h2>Items List</h2>
      <ul style={{ padding: 0, listStyle: 'none' }}>
        {products.map(p => (
          <li key={p.id} style={{ display: 'flex', justifyContent: 'space-between', padding: '10px', borderBottom: '1px solid #ccc' }}>
            <div>
              <strong>{p.name}</strong> (${p.price}) <br />
              <small style={{ color: '#666' }}>{p.description}</small>
            </div>
            <button onClick={() => p.id && handleDelete(p.id)} style={{ background: 'red', color: 'white', border: 'none', padding: '5px 10px', cursor: 'pointer' }}>Delete</button>
          </li>
        ))}
      </ul>
    </main>
  );
}

