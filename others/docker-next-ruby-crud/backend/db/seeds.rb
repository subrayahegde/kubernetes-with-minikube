Product.find_or_create_by!(name: 'Premium Laptop') do |product|
  product.description = 'High-performance workstation'
  product.price = 1299.99
end

Product.find_or_create_by!(name: 'Wireless Mouse') do |product|
  product.description = 'Ergonomic 2.4GHz optical mouse'
  product.price = 29.99
end
