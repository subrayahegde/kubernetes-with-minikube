<?php

use Illuminate\Support\Facades\Route;
use App\Http\Controllers\ProductController;

/*
|--------------------------------------------------------------------------
| Web Routes
|--------------------------------------------------------------------------
*/


// 9. Resource Controller (Automatically maps standard CRUD endpoints)
// Creates routes for index, create, store, show, edit, update, destroy
Route::resource('/products', ProductController::class);
