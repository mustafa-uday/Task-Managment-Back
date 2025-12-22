#!/bin/bash

echo "=== Database Connection Diagnostic ==="
echo ""

# Check if PostgreSQL is running
echo "1. Checking if PostgreSQL is running..."
if lsof -i :5432 > /dev/null 2>&1; then
    echo "   ✓ PostgreSQL is running on port 5432"
else
    echo "   ✗ PostgreSQL is NOT running on port 5432"
    echo "   → Start PostgreSQL: brew services start postgresql (macOS) or sudo systemctl start postgresql (Linux)"
    exit 1
fi

echo ""
echo "2. Testing PostgreSQL connection..."
if psql -U postgres -c "SELECT version();" > /dev/null 2>&1; then
    echo "   ✓ Can connect to PostgreSQL"
else
    echo "   ✗ Cannot connect to PostgreSQL"
    echo "   → Check your PostgreSQL credentials"
    exit 1
fi

echo ""
echo "3. Checking if TaskManagerDb exists..."
if psql -U postgres -lqt | cut -d \| -f 1 | grep -qw TaskManagerDb; then
    echo "   ✓ Database 'TaskManagerDb' exists"
else
    echo "   ✗ Database 'TaskManagerDb' does NOT exist"
    echo ""
    echo "   Creating database now..."
    psql -U postgres -c "CREATE DATABASE TaskManagerDb;" 2>&1
    if [ $? -eq 0 ]; then
        echo "   ✓ Database created successfully!"
    else
        echo "   ✗ Failed to create database"
        echo "   → Run manually: psql -U postgres -c 'CREATE DATABASE TaskManagerDb;'"
        exit 1
    fi
fi

echo ""
echo "4. Testing connection to TaskManagerDb..."
if psql -U postgres -d TaskManagerDb -c "SELECT 1;" > /dev/null 2>&1; then
    echo "   ✓ Can connect to TaskManagerDb"
else
    echo "   ✗ Cannot connect to TaskManagerDb"
    exit 1
fi

echo ""
echo "=== All checks passed! ==="
echo "Your database is ready. You can now run your application."


