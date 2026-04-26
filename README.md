# WindowSill Disk I/O - Complete DevOps Kit

Extensão para exibir métricas de I/O de disco na barra do WindowSill.

---

## Pipeline local

### Build + Pack
```powershell
.\build-and-pack.ps1
```

Gera `artifacts/WindowSill.DiskIo.X.Y.Z.nupkg` e `artifacts/WindowSill.DiskIo.X.Y.Z.wsext`.

### Instalar
```powershell
.\install-diskio-extension.ps1 -ExtensionPath .\artifacts\WindowSill.DiskIo.*.wsext
```

---

## CI/CD GitHub

- Push `main`: build, pack, upload `.nupkg` + `.wsext`.
- Release `v*`: publica `.nupkg` no NuGet.

---

## Fluxo

1. Build gera `.nupkg`.
2. Script renomeia para `.wsext`.
3. Installer valida política, registra WindowSill, instala `.wsext`.

**Configurar NUGET_API_KEY em GitHub Secrets para publicação automática.**
