# Troubleshooting Guide

Common issues and solutions for the Sea of Thieves Microservices Demo.

## 📚 Table of Contents

1. [Installation Issues](#installation-issues)
2. [Docker Issues](#docker-issues)
3. [Service Startup Issues](#service-startup-issues)
4. [API Access Issues](#api-access-issues)
5. [Event Issues](#event-issues)
6. [Performance Issues](#performance-issues)
7. [Common Error Messages](#common-error-messages)

---

## Installation Issues

### Issue: Docker Desktop Won't Install

**Symptoms**:
- Installer fails
- Error messages during installation
- Installation hangs

**Solutions**:

1. **Check System Requirements**:
   - Windows 10 version 2004 or later
   - 64-bit processor
   - Virtualization enabled in BIOS

2. **Enable Virtualization**:
   - Restart computer
   - Enter BIOS/UEFI settings (usually F2, F10, or Del during boot)
   - Enable "Virtualization Technology" or "Intel VT-x" / "AMD-V"
   - Save and exit

3. **Check Windows Features**:
   - Open "Turn Windows features on or off"
   - Ensure "Hyper-V" or "Windows Subsystem for Linux" is enabled
   - Restart if changes were made

4. **Run as Administrator**:
   - Right-click Docker Desktop installer
   - Select "Run as administrator"

### Issue: Docker Desktop Won't Start

**Symptoms**:
- Docker Desktop icon shows error
- "Docker Desktop is starting..." never completes
- Error messages about WSL or Hyper-V

**Solutions**:

1. **Check WSL 2**:
   ```powershell
   wsl --status
   ```
   If not installed:
   ```powershell
   wsl --install
   ```
   Restart computer after installation

2. **Restart Docker Desktop**:
   - Right-click Docker icon in system tray
   - Select "Restart"
   - Wait 2-3 minutes

3. **Check Resources**:
   - Open Docker Desktop Settings
   - Go to "Resources"
   - Ensure at least 4GB RAM allocated
   - Ensure at least 20GB disk space

4. **Reset Docker Desktop**:
   - Docker Desktop Settings → Troubleshoot → Reset to factory defaults
   - **Warning**: This removes all containers and images

---

## Docker Issues

### Issue: "Port Already in Use"

**Symptoms**:
```
Error: bind: address already in use
```

**Solutions**:

1. **Find What's Using the Port**:
   ```powershell
   netstat -ano | findstr :5000
   ```
   Note the PID (last number)

2. **Stop the Process**:
   ```powershell
   taskkill /PID [PID] /F
   ```

3. **Or Change Port in docker-compose.yml**:
   ```yaml
   ports:
     - "5001:8080"  # Change 5001 to another port
   ```

### Issue: "Cannot Connect to Docker Daemon"

**Symptoms**:
- `docker` commands fail
- "Cannot connect to the Docker daemon"

**Solutions**:

1. **Check Docker Desktop is Running**:
   - Look for Docker icon in system tray
   - If not there, start Docker Desktop

2. **Restart Docker Desktop**:
   - Right-click Docker icon → Restart

3. **Verify Docker is Running**:
   ```powershell
   docker ps
   ```
   Should show running containers or empty list (not an error)

### Issue: "Out of Disk Space"

**Symptoms**:
- Docker errors about disk space
- Containers fail to start

**Solutions**:

1. **Check Disk Space**:
   ```powershell
   docker system df
   ```

2. **Clean Up Unused Resources**:
   ```powershell
   docker system prune -a
   ```
   **Warning**: Removes all unused images, containers, networks

3. **Remove Specific Images**:
   ```powershell
   docker image prune -a
   ```

---

## Service Startup Issues

### Issue: Services Won't Start

**Symptoms**:
- `docker-compose ps` shows services as "Exited" or "Restarting"
- Services show errors in logs

**Solutions**:

1. **Check Service Logs**:
   ```powershell
   docker-compose logs [service-name]
   ```
   Example:
   ```powershell
   docker-compose logs player-service
   ```

2. **Check All Logs**:
   ```powershell
   docker-compose logs
   ```

3. **Restart Specific Service**:
   ```powershell
   docker-compose restart [service-name]
   ```

4. **Rebuild and Restart**:
   ```powershell
   docker-compose up -d --build [service-name]
   ```

### Issue: Database Connection Errors

**Symptoms**:
- Services show database connection errors
- "Cannot open database" errors

**Solutions**:

1. **Wait for Database to Start**:
   ```powershell
   docker-compose logs azuresql
   ```
   Wait until you see "SQL Server is ready"

2. **Check Database Container**:
   ```powershell
   docker-compose ps azuresql
   ```
   Should show "Up"

3. **Restart Database**:
   ```powershell
   docker-compose restart azuresql
   ```
   Wait 30 seconds, then restart services

4. **Check Connection String**:
   - Verify `docker-compose.yml` has correct connection settings
   - Default: `Server=azuresql;Database=...`

### Issue: RabbitMQ Connection Errors

**Symptoms**:
- Services show RabbitMQ connection errors
- "Failed to connect to RabbitMQ" in logs

**Solutions**:

1. **Check RabbitMQ Container**:
   ```powershell
   docker-compose ps rabbitmq
   ```

2. **Check RabbitMQ Logs**:
   ```powershell
   docker-compose logs rabbitmq
   ```

3. **Restart RabbitMQ**:
   ```powershell
   docker-compose restart rabbitmq
   ```
   Wait 30 seconds

4. **Verify RabbitMQ is Accessible**:
   - Open browser: `http://localhost:15672`
   - Login: `guest` / `guest`
   - Should see management interface

---

## API Access Issues

### Issue: "Cannot Connect to API Gateway"

**Symptoms**:
- Browser shows "This site can't be reached"
- PowerShell shows connection errors

**Solutions**:

1. **Check API Gateway is Running**:
   ```powershell
   docker-compose ps api-gateway
   ```

2. **Check API Gateway Logs**:
   ```powershell
   docker-compose logs api-gateway
   ```

3. **Test Direct Access**:
   ```powershell
   curl http://localhost:5000/health
   ```

4. **Check Port is Not Blocked**:
   - Windows Firewall may block port 5000
   - Temporarily disable firewall to test
   - Or add exception for port 5000

### Issue: "404 Not Found" Errors

**Symptoms**:
- API returns 404 for valid endpoints

**Solutions**:

1. **Check Route Configuration**:
   - Verify `src/ApiGateway/ocelot.json` has correct routes
   - Ensure route matches endpoint

2. **Check Service is Running**:
   ```powershell
   docker-compose ps [service-name]
   ```

3. **Test Direct Service Access**:
   ```powershell
   curl http://localhost:5001/api/players
   ```
   If this works, issue is with API Gateway routing

4. **Restart API Gateway**:
   ```powershell
   docker-compose restart api-gateway
   ```

### Issue: "500 Internal Server Error"

**Symptoms**:
- API returns 500 errors
- Services show exceptions in logs

**Solutions**:

1. **Check Service Logs**:
   ```powershell
   docker-compose logs [service-name]
   ```
   Look for exception messages

2. **Check Database Connection**:
   - Verify database is running
   - Check connection strings

3. **Check Data Validation**:
   - Ensure request body matches expected format
   - Check required fields are provided

4. **Restart Service**:
   ```powershell
   docker-compose restart [service-name]
   ```

---

## Event Issues

### Issue: Events Not Appearing

**Symptoms**:
- Event Consumer shows no events
- No events in RabbitMQ

**Solutions**:

1. **Check Event Consumer is Running**:
   ```powershell
   docker-compose ps event-consumer
   ```

2. **Check Event Consumer Logs**:
   ```powershell
   docker-compose logs event-consumer
   ```
   Should show "Connected to RabbitMQ"

3. **Verify RabbitMQ Exchange**:
   - Open `http://localhost:15672`
   - Go to "Exchanges"
   - Look for `seaofthieves.events`

4. **Check Queue Binding**:
   - In RabbitMQ UI, go to "Queues"
   - Look for `event-consumer-queue`
   - Check it's bound to `seaofthieves.events`

5. **Trigger Test Event**:
   ```powershell
   $player = @{ Name = "Test"; Gamertag = "Test"; Gold = 100; Renown = 1; IsPirateLegend = $false; Platform = "PC" } | ConvertTo-Json
   Invoke-RestMethod -Uri "http://localhost:5000/api/players" -Method Post -Body $player -ContentType "application/json"
   ```
   Then check Event Consumer logs

### Issue: Events Not Being Published

**Symptoms**:
- Services show "Failed to publish" in logs
- No events in RabbitMQ

**Solutions**:

1. **Check RabbitMQ Connection**:
   - Verify RabbitMQ is running
   - Check service logs for connection errors

2. **Check Environment Variables**:
   - Services use `RABBITMQ_HOST=rabbitmq`
   - Verify this matches docker-compose service name

3. **Restart Services**:
   ```powershell
   docker-compose restart
   ```

---

## Performance Issues

### Issue: Services Are Slow

**Symptoms**:
- API calls take a long time
- Services respond slowly

**Solutions**:

1. **Check System Resources**:
   - Open Task Manager
   - Check CPU and Memory usage
   - Docker Desktop may need more resources

2. **Allocate More Resources to Docker**:
   - Docker Desktop Settings → Resources
   - Increase CPU and Memory allocation
   - Restart Docker Desktop

3. **Check Database Performance**:
   ```powershell
   docker-compose logs azuresql
   ```
   Look for slow query warnings

4. **Reduce Number of Services**:
   - Comment out unused services in `docker-compose.yml`
   - Restart with fewer services

### Issue: Out of Memory

**Symptoms**:
- Services crash
- Docker errors about memory

**Solutions**:

1. **Check Memory Usage**:
   ```powershell
   docker stats
   ```

2. **Reduce Docker Memory**:
   - Docker Desktop Settings → Resources
   - Reduce memory allocation
   - Restart Docker Desktop

3. **Stop Unused Containers**:
   ```powershell
   docker-compose down
   docker-compose up -d [specific-services]
   ```

---

## Common Error Messages

### "Service Unavailable"

**Cause**: Service is not running or not ready

**Solution**:
```powershell
docker-compose ps
docker-compose restart [service-name]
```

### "Connection Refused"

**Cause**: Port is not accessible or service is down

**Solution**:
- Check service is running
- Check port is not blocked by firewall
- Verify port mapping in docker-compose.yml

### "Database Initialization Failed"

**Cause**: Database not ready or connection issue

**Solution**:
```powershell
docker-compose restart azuresql
# Wait 30 seconds
docker-compose restart [service-name]
```

### "RabbitMQ Connection Failed"

**Cause**: RabbitMQ not ready or network issue

**Solution**:
```powershell
docker-compose restart rabbitmq
# Wait 30 seconds
docker-compose restart [service-name]
```

---

## Getting Help

### Diagnostic Information to Collect

If you need help, collect this information:

1. **Docker Version**:
   ```powershell
   docker --version
   docker-compose --version
   ```

2. **Service Status**:
   ```powershell
   docker-compose ps
   ```

3. **Recent Logs**:
   ```powershell
   docker-compose logs --tail=50
   ```

4. **System Information**:
   - Windows version
   - Available RAM
   - Available disk space

5. **Error Messages**:
   - Copy exact error messages
   - Include stack traces if available

### Useful Commands for Diagnostics

```powershell
# Check all services
docker-compose ps

# View all logs
docker-compose logs

# Check Docker system
docker system df

# Check container resources
docker stats

# Check network
docker network ls
docker network inspect [network-name]
```

---

## Prevention Tips

1. **Regular Cleanup**:
   ```powershell
   docker system prune -a
   ```

2. **Monitor Resources**:
   - Keep an eye on Docker Desktop resource usage
   - Adjust allocations as needed

3. **Keep Docker Updated**:
   - Update Docker Desktop regularly
   - Check for updates in Docker Desktop

4. **Backup Important Data**:
   - Export data before major changes
   - Keep docker-compose.yml backed up

---

**Still having issues? Check the logs first, then try restarting the affected services!** ⚓

