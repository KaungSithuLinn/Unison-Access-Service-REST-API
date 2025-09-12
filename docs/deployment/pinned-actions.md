# Pinned GitHub Actions for Security

This document contains the specific commit SHAs for GitHub Actions used in our workflows.
These are pinned for security purposes to prevent supply chain attacks.

## Action Versions and SHAs

### Core Actions (GitHub Official)

- `actions/checkout@v4` → `actions/checkout@b4ffde65f46336ab88eb53be808477a3936bae11` # v4.1.1
- `actions/setup-dotnet@v4` → `actions/setup-dotnet@4d6c8fcf3c8f7a60068d26b594648e99df24cee3` # v4.0.0
- `actions/upload-artifact@v4` → `actions/upload-artifact@5d5d22a31266ced268874388b861e4b58bb5c2f3` # v4.3.1
- `actions/download-artifact@v4` → `actions/download-artifact@c850b930e6ba138125429b7e5c93fc707a7f8427` # v4.1.4

### Azure Actions

- `azure/login@v1` → `azure/login@92a5484dfaf04ca78a94597f4f19fea633851fa2` # v1.4.7
- `azure/webapps-deploy@v3` → `azure/webapps-deploy@85270a1854658d167ab239bce43949edb336fa7c` # v3.0.1

### Docker Actions

- `docker/setup-buildx-action@v3` → `docker/setup-buildx-action@f95db51fddba0c2d1ec667646a06c2ce06100226` # v3.0.0
- `docker/metadata-action@v5` → `docker/metadata-action@8e5442c4ef9f78752691e2d8f8d19755c6f78e81` # v5.5.1
- `docker/build-push-action@v5` → `docker/build-push-action@0565240e2d4ab88bba5387d719585280857ece09` # v5.0.0

### HashiCorp Actions

- `hashicorp/setup-terraform@v3` → `hashicorp/setup-terraform@a1502cd9e758c50496cc9ac5308c4843bcd56d36` # v3.0.0

## Update Process

1. Check for new releases monthly
2. Update SHAs when security patches are available
3. Test workflows after updates
4. Update this documentation

## Security Verification

Before updating, verify the SHA corresponds to the expected tag:

```bash
# Example for actions/checkout
git ls-remote --tags https://github.com/actions/checkout
# Verify the SHA matches the tag you want to use
```
