#!/bin/bash

echo "=== Creating Database Migrations ==="
echo ""

# Navigate to project root
cd "$(dirname "$0")"

echo "Step 1: Installing EF Core tools (if needed)..."
if ! dotnet ef --version > /dev/null 2>&1; then
    echo "Installing dotnet-ef tool..."
    dotnet tool install --global dotnet-ef --version 9.0.0 || {
        echo "Failed to install dotnet-ef. Trying alternative method..."
        # Try installing locally
        dotnet new tool-manifest 2>/dev/null
        dotnet tool install dotnet-ef --version 9.0.0
    }
else
    echo "✓ EF Core tools already installed"
fi

echo ""
echo "Step 2: Creating initial migration..."
cd TaskManager.Infrastructure

# Try with global tool first
if dotnet ef migrations add InitialCreate --startup-project ../TaskManager.Api --context ApplicationDbContext 2>/dev/null; then
    echo "✓ Migration created successfully"
elif dotnet tool run dotnet-ef migrations add InitialCreate --startup-project ../TaskManager.Api --context ApplicationDbContext 2>/dev/null; then
    echo "✓ Migration created successfully (using local tool)"
else
    echo "✗ Failed to create migration"
    echo ""
    echo "Please run manually:"
    echo "  cd TaskManager.Infrastructure"
    echo "  dotnet ef migrations add InitialCreate --startup-project ../TaskManager.Api --context ApplicationDbContext"
    exit 1
fi

echo ""
echo "Step 3: Applying migration to database..."
cd ../TaskManager.Api

if dotnet ef database update --project ../TaskManager.Infrastructure --context ApplicationDbContext 2>/dev/null; then
    echo "✓ Database updated successfully"
elif dotnet tool run dotnet-ef database update --project ../TaskManager.Infrastructure --context ApplicationDbContext 2>/dev/null; then
    echo "✓ Database updated successfully (using local tool)"
else
    echo "✗ Failed to update database"
    echo ""
    echo "The migration will be applied automatically when you run the application."
    echo "Or run manually:"
    echo "  cd TaskManager.Api"
    echo "  dotnet ef database update --project ../TaskManager.Infrastructure --context ApplicationDbContext"
    exit 1
fi

echo ""
echo "=== Migration Complete! ==="
echo "Your database schema has been created."


