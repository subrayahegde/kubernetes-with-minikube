Rails.application.routes.draw do
  namespace :api do
    namespace :v1 do
      resources :products, only: [:index, :show, :create, :update, :destroy]
    end
  end

  scope '/api', module: 'api/v1' do
    resources :products, only: [:index, :show, :create, :update, :destroy]
  end
end
