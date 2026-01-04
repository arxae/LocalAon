set shell := ["powershell.exe", "-c"]

default:
    just -l

# Entity framework migrations because im lazy

# Applies EF migration
migrate:
    dotnet ef database update --project src/LocalAon.Scraper

# Create a new EF migration
newmigration NAME:
    dotnet ef migrations add {{NAME}} --project src/LocalAon.Scraper

# List migrations
migrations:
    dotnet ef migrations list --project src/LocalAon.Scraper
