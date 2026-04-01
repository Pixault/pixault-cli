# Pixault CLI

A rich terminal tool for the [Pixault](https://pixault.io) image CDN. Upload, manage, and transform images from the command line.

## Install

```bash
dotnet tool install -g Pixault.Cli
```

## Quick Start

```bash
# Configure your API credentials
pixault config init

# Upload an image
pixault upload photo.jpg --alt "My photo" --tags "nature,landscape"

# List images
pixault images list --limit 10

# Generate a CDN URL with transforms
pixault images url img_01JKXYZ -w 800 -q 85 --format webp
```

## Commands

```
pixault config init|show         Configuration management
pixault upload <path>            Upload files (supports globs)
pixault images list|get|update|delete|url|derived
pixault folders list|create|delete
pixault transforms list|get|create|delete
pixault plugins list|installed|activate|deactivate
pixault images embed <id>        Generate SEO embed tags
pixault eps status|split|extract-svg   EPS processing
```

Use `pixault --help` or `pixault <command> --help` for details.

### SEO Embed Tags

```bash
pixault images embed <id> --alt "Description" --widths 400,800,1200
pixault images embed <id> --alt "Portfolio" -t gallery --no-picture
```

## Documentation

Full documentation at [pixault.dev](https://pixault.dev).
