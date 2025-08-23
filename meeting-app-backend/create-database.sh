#!/bin/bash
set -e

echo "Entity Framework Core migrasyon ve veritabanı oluşturma başlatılıyor..."

# .NET Tool'ları yükle
dotnet tool install --global dotnet-ef || true

# Migration oluştur
echo "Migrations oluşturuluyor..."
dotnet ef migrations add InitialCreate --project Company.Project.Context --startup-project Company.Project.Api

# Veritabanını oluştur
echo "Veritabanı oluşturuluyor..."
dotnet ef database update --project Company.Project.Context --startup-project Company.Project.Api

echo "Veritabanı başarıyla oluşturuldu!"
