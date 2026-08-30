<?php

use Illuminate\Support\Facades\Route;
use App\Http\Controllers\ProductController;

Route::get('/products', [ProductController::class, 'index']);

Route::get('/status', function () {
    return response()->json([
        'status' => 'success',
        'message' => 'Laravel backend is connected successfully to PostgreSQL'
    ]);
});

