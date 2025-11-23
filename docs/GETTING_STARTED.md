# Getting Started Guide

This guide will walk you through installing everything you need and getting the Sea of Thieves Microservices Demo running from absolute zero.

## 📋 Table of Contents

1. [Prerequisites Check](#prerequisites-check)
2. [Installing Docker Desktop](#installing-docker-desktop)
3. [Downloading the Project](#downloading-the-project)
4. [Starting the Services](#starting-the-services)
5. [Verifying Everything Works](#verifying-everything-works)
6. [Next Steps](#next-steps)

---

## Prerequisites Check

Before we begin, let's make sure your computer meets the requirements:

### System Requirements

- **Operating System**: Windows 10 (version 2004 or later) or Windows 11
- **RAM**: Minimum 8GB (16GB recommended)
- **Disk Space**: At least 10GB free space
- **Processor**: 64-bit processor with virtualization support

### Check Your Windows Version

1. Press `Windows Key + R`
2. Type `winver` and press Enter
3. Check that you have Windows 10 version 2004 or later, or Windows 11

### Check Available Disk Space

1. Open File Explorer
2. Right-click on `C:` drive
3. Select `Properties`
4. Check "Free space" - you need at least 10GB

---

## Installing Docker Desktop

Docker Desktop is the software that will run all our services. Follow these steps carefully.

### Step 1: Download Docker Desktop

1. Open your web browser
2. Go to: https://www.docker.com/products/docker-desktop/
3. Click the **"Download for Windows"** button
4. The installer file will download (it's about 500MB, so this may take a few minutes)

### Step 2: Install Docker Desktop

1. **Locate the downloaded file** (usually in your `Downloads` folder)
   - The file is named something like `Docker Desktop Installer.exe`

2. **Double-click the installer** to start installation

3. **Follow the installation wizard**:
   - ✅ Check "Use WSL 2 instead of Hyper-V" (if available)
   - ✅ Check "Add shortcut to desktop" (optional but helpful)
   - Click **"Ok"** to begin installation

4. **Wait for installation to complete** (this may take 5-10 minutes)

5. **When prompted, click "Close and restart"** to restart your computer

### Step 3: Start Docker Desktop

1. **After your computer restarts**, look for the Docker Desktop icon on your desktop or in the Start menu
2. **Double-click** to launch Docker Desktop
3. **Accept the service agreement** if prompted
4. **Wait for Docker Desktop to start** (you'll see a whale icon in your system tray when it's ready)
   - This may take 1-2 minutes the first time
   - The icon will stop animating when Docker is ready

### Step 4: Verify Docker Desktop is Running

1. **Right-click the Docker icon** in your system tray (bottom-right corner)
2. **Click "Settings"** or look for the Docker Desktop window
3. You should see "Docker Desktop is running" or a green status indicator

**Troubleshooting**: If Docker Desktop won't start, see the [Troubleshooting Guide](../docs/TROUBLESHOOTING.md)

---

## Downloading the Project

You have two options to get the project files:

### Option 1: If You Have Git Installed

1. Open PowerShell (search for "PowerShell" in Start menu)
2. Navigate to where you want the project (e.g., `cd C:\Users\YourName\Documents`)
3. Run:
   ```powershell
   git clone [repository-url]
   cd microservices-demo-sot
   ```

### Option 2: Download as ZIP

1. Download the project as a ZIP file
2. **Extract the ZIP file** to a location like `C:\Users\YourName\Documents\microservices-demo-sot`
3. **Open PowerShell** in that folder:
   - Right-click in the folder
   - Select "Open in Terminal" or "Open PowerShell window here"

### Verify You're in the Right Place

In PowerShell, you should see a path ending with `microservices-demo-sot`. You can verify by running:

```powershell
Get-Location
```

You should see files like `docker-compose.yml` and `README.md` when you list the directory:

```powershell
Get-ChildItem
```

---

## Starting the Services

Now that everything is installed, let's start all the services!

### Step 1: Open PowerShell in the Project Folder

1. Navigate to the project folder in File Explorer
2. **Right-click** in an empty area
3. Select **"Open in Terminal"** or **"Open PowerShell window here"**

### Step 2: Start All Services

Type this command and press Enter:

```powershell
docker-compose up -d
```

**What this does:**
- `docker-compose` - Tells Docker to manage multiple services
- `up` - Start all services
- `-d` - Run in the background (detached mode)

**What to expect:**
- Docker will start downloading images (first time only - this may take 10-20 minutes)
- You'll see lots of text scrolling by
- This is normal! Docker is building and starting 15+ containers
- Wait until you see messages like "Creating...", "Starting...", and "Started"

### Step 3: Wait for Services to Start

Services need time to start up. Wait about **2-3 minutes** after the `docker-compose up -d` command completes.

**How to check if services are ready:**

```powershell
docker-compose ps
```

You should see all services listed with status "Up" or "healthy". If some show "health: starting", wait another minute and check again.

### Step 4: View Service Logs (Optional)

To see what's happening with a specific service:

```powershell
docker-compose logs player-service
```

Or to see all logs:

```powershell
docker-compose logs
```

Press `Ctrl+C` to stop viewing logs.

---

## Verifying Everything Works

Let's make sure everything is running correctly!

### Test 1: Check API Gateway

Open your web browser and go to:

```
http://localhost:5000/api/players
```

**Expected result**: You should see a JSON list of players (may be empty `[]` or contain sample data)

**If you see an error**: Wait another minute and try again, or check the [Troubleshooting Guide](../docs/TROUBLESHOOTING.md)

### Test 2: Check RabbitMQ Management

1. Open your web browser
2. Go to: `http://localhost:15672`
3. Login with:
   - **Username**: `guest`
   - **Password**: `guest`

**Expected result**: You should see the RabbitMQ management dashboard

### Test 3: Check Service Health

In PowerShell, run:

```powershell
curl http://localhost:5000/health
```

**Expected result**: Should return "Healthy"

### Test 4: List All Running Services

```powershell
docker-compose ps
```

**Expected result**: You should see about 15 services listed, most showing "Up" or "healthy"

---

## Next Steps

Congratulations! 🎉 Your microservices demo is now running!

### What to Do Next

1. **Learn how to use it**: Read the [User Guide](USER_GUIDE.md)
2. **See demonstrations**: Check the [Demonstration Guide](DEMONSTRATIONS.md)
3. **Understand the architecture**: Read the [Architecture Overview](ARCHITECTURE.md)

### Common Commands

**Stop all services:**
```powershell
docker-compose down
```

**Restart all services:**
```powershell
docker-compose restart
```

**View logs for a service:**
```powershell
docker-compose logs [service-name]
```

**Check service status:**
```powershell
docker-compose ps
```

### Services You Can Access

- **API Gateway**: http://localhost:5000
- **RabbitMQ Management**: http://localhost:15672 (guest/guest)
- **Individual Services**: http://localhost:5001 through http://localhost:5012

---

## Troubleshooting

If something didn't work:

1. **Docker Desktop not running?**
   - Make sure Docker Desktop is started (check system tray)
   - Wait for the whale icon to stop animating

2. **Port already in use?**
   - Close any applications using ports 5000-5012 or 15672
   - Restart Docker Desktop

3. **Services won't start?**
   - Check Docker Desktop has enough resources (Settings → Resources)
   - Ensure virtualization is enabled in BIOS

4. **Need more help?**
   - See the [Troubleshooting Guide](TROUBLESHOOTING.md)
   - Check service logs: `docker-compose logs [service-name]`

---

**You're all set! Ready to explore the microservices?** ⚓

