#!/bin/bash

# Ensure gdformat is installed and accessible
if ! command -v gdformat &> /dev/null; then
  echo "gdformat not found. Please ensure it is installed and available in your PATH."
  exit 1
fi

# Loop through the current directory and all subdirectories, excluding "addons"
find . -type f -name "*.gd" ! -path "./addons/*" | while read -r file; do
  echo "Formatting: $file"
  gdformat "$file" --line-length 200
done

echo "Formatting complete!"
