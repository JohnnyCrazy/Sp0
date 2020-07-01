#!/bin/bash

set -e

cd Sp0.Core
dotnet warp -o sp0-dev
chmod +x sp0-dev
sudo mv sp0-dev /usr/local/bin/sp0-dev
cd ..
