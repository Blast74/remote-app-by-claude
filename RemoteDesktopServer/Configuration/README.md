# Configuration du Serveur RemoteDesktop

## Vue d'ensemble

Le serveur RemoteDesktop utilise un système de configuration flexible basé sur plusieurs fichiers JSON qui permettent de modifier les paramètres sans recompiler l'application.

## Fichiers de Configuration

### 1. `default-settings.json`
Contient toutes les valeurs par défaut pour chaque section de configuration. Ce fichier sert de fallback lorsque des valeurs ne sont pas spécifiées dans les autres fichiers.

### 2. `appsettings.json`
Configuration de base pour tous les environnements.

### 3. `appsettings.Development.json`
Configuration spécifique à l'environnement de développement. Remplace les valeurs d'`appsettings.json`.

### 4. `appsettings.Production.json`
Configuration spécifique à l'environnement de production. Remplace les valeurs d'`appsettings.json`.

## Priorité de Configuration

Les paramètres sont chargés dans l'ordre suivant (du moins prioritaire au plus prioritaire) :

1. **default-settings.json** - Valeurs par défaut
2. **appsettings.json** - Configuration de base
3. **appsettings.{Environment}.json** - Configuration spécifique à l'environnement
4. **Variables d'environnement** - Remplacent toute autre configuration

## Sections de Configuration

### ServerSettings
```json
{
  "ServerSettings": {
    "ServerName": "RDP-SERVER-01",
    "ListenPort": 3389,
    "MaxConcurrentSessions": 50,
    "SessionTimeoutMinutes": 480,
    "MaxIdleTimeMinutes": 30,
    "EnableSessionRecording": false,
    "RecordingPath": "recordings",
    "TempPath": "temp",
    "LogsPath": "logs"
  }
}
```

### SecuritySettings
```json
{
  "Security": {
    "EncryptionLevel": "High",
    "EncryptionKey": "VotreCleDeChiffrement",
    "RequireNLA": true,
    "AllowedAuthMethods": ["NTLM", "Kerberos", "Certificate"],
    "CertificateThumbprint": "",
    "RequireSecureRPC": true,
    "EnableAuditLogging": true,
    "FailedLoginAttempts": 3,
    "LockoutDurationMinutes": 15,
    "PasswordComplexity": true,
    "SessionEncryption": "AES256"
  }
}
```

### PerformanceSettings
```json
{
  "Performance": {
    "MonitoringInterval": 5000,
    "PerformanceCountersEnabled": true,
    "MaxCpuUsage": 85.0,
    "MaxMemoryUsage": 90.0,
    "MaxDiskUsage": 95.0,
    "AlertThresholds": {
      "CpuCritical": 95.0,
      "MemoryCritical": 95.0,
      "DiskCritical": 98.0
    }
  }
}
```

## Modification de Configuration Sans Recompilation

### Méthode 1: Modifier appsettings.json
Pour modifier la configuration en cours d'exécution, éditez le fichier `appsettings.json` ou `appsettings.Production.json` et redémarrez le service.

### Méthode 2: Variables d'environnement
Utilisez des variables d'environnement pour remplacer des valeurs spécifiques :

```bash
# Exemple pour modifier le port d'écoute
export ServerSettings__ListenPort=3390

# Exemple pour modifier la clé de chiffrement
export Security__EncryptionKey="NouvelleCleSecurisee123456789"

# Exemple pour Active Directory
export ActiveDirectory__Domain="votre-domaine.com"
export ActiveDirectory__LdapPath="LDAP://votre-dc.votre-domaine.com"
```

### Méthode 3: Modifier default-settings.json
Pour changer les valeurs par défaut globales, modifiez le fichier `Configuration/default-settings.json`.

## Exemples d'Usage

### Configuration pour un environnement de test
```json
{
  "ServerSettings": {
    "ServerName": "RDP-TEST-SERVER",
    "ListenPort": 3391,
    "MaxConcurrentSessions": 5
  },
  "Security": {
    "FailedLoginAttempts": 10,
    "LockoutDurationMinutes": 5
  }
}
```

### Configuration pour haute disponibilité
```json
{
  "ServerSettings": {
    "MaxConcurrentSessions": 200
  },
  "Performance": {
    "MonitoringInterval": 3000,
    "MaxCpuUsage": 70.0,
    "MaxMemoryUsage": 80.0
  },
  "LoadBalancing": {
    "Enabled": true,
    "ServerFarm": ["server1.local", "server2.local"],
    "LoadBalanceMethod": "LeastConnections"
  }
}
```

## Sécurité

⚠️ **Important** : Ne jamais commiter des clés de chiffrement ou des secrets en production dans les fichiers de configuration. Utilisez des variables d'environnement ou des solutions de gestion de secrets comme Azure Key Vault.

### Bonnes pratiques :
1. Utilisez des variables d'environnement pour les secrets
2. Chiffrez les fichiers de configuration sensibles
3. Limitez l'accès aux fichiers de configuration
4. Auditez les modifications de configuration

## Redémarrage du Service

Après modification des fichiers JSON, redémarrez le service :

```bash
# Service Windows
net stop RemoteDesktopServer
net start RemoteDesktopServer

# Service systemd (Linux)
sudo systemctl restart remotedesktopserver

# Mode développement
dotnet run
```

## Validation de Configuration

Le serveur valide automatiquement la configuration au démarrage et applique les valeurs par défaut pour les paramètres manquants. Consultez les logs pour voir la configuration effective utilisée.