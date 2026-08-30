from django.db import models

class Product(models.Model):
    id = models.AutoField(primary_key=True)
    # Fixed line below: removed the invalid max_str_len argument
    name = models.CharField(max_length=100)
    description = models.TextField(blank=True, null=True)
    price = models.DecimalField(max_digits=10, decimal_places=2)
    created_at = models.DateTimeField(auto_now_add=True)

    class Meta:
        db_table = 'products'

    def __str__(self):
        return self.name

