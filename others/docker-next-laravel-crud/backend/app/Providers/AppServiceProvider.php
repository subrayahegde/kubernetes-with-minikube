<?php

namespace App\Providers;

use Illuminate\Support\ServiceProvider;
use Illuminate\Support\Facades\Route; // ➜ ADD THIS LINE

class AppServiceProvider extends ServiceProvider
{
    public function register(): void {}
    public function boot(): void { 
     Route::middleware('api')
    ->prefix('api') // Ensure this line exists
    ->group(function () {
            // This single line opens up GET, POST, PUT, and DELETE for products
            Route::resource('products', \App\Http\Controllers\ProductController::class);
        });
    }
}


  
       
