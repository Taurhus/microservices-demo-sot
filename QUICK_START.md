# Quick Start Guide

**For users who want to get started quickly!**

## Prerequisites

- ✅ Docker Desktop installed and running
- ✅ PowerShell available

## 3-Step Setup

### Step 1: Open PowerShell

1. Navigate to the project folder
2. Right-click → "Open in Terminal" or "Open PowerShell window here"

### Step 2: Start Everything

```powershell
docker-compose up -d
```

Wait 2-3 minutes for all services to start.

### Step 3: Verify It's Working

Open your browser and go to:
```
http://localhost:5000/api/players
```

If you see JSON data (even if it's just `[]`), you're all set! ✅

## What's Next?

- **New to this?** → Read [Getting Started Guide](docs/GETTING_STARTED.md)
- **Want to use it?** → Read [User Guide](docs/USER_GUIDE.md)
- **Want demos?** → Read [Demonstration Guide](docs/DEMONSTRATIONS.md)

## Common Commands

**Stop everything:**
```powershell
docker-compose down
```

**Restart everything:**
```powershell
docker-compose restart
```

**Check status:**
```powershell
docker-compose ps
```

**View logs:**
```powershell
docker-compose logs
```

## Need Help?

See the [Troubleshooting Guide](docs/TROUBLESHOOTING.md) for common issues.

---

**That's it! You're ready to explore!** ⚓

