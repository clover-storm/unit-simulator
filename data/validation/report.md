# Schema Validation Report

Generated: 2026-01-12

## Validation Summary

| Schema File | Data File | Status | Issues |
|-------------|-----------|--------|--------|
| `unit-stats.schema.json` | `data/references/units.json` | ✅ VALID | 0 |
| `skill-reference.schema.json` | `data/references/skills.json` | ✅ VALID | 0 |
| `tower-reference.schema.json` | `data/references/towers.json` | ✅ VALID | 0 |
| `wave-definition.schema.json` | `data/references/waves.json` | ⚠️ N/A | File does not exist yet |

## Validation Details

### units.json
- **Total Units**: 13
- **Schema Version**: JSON Schema Draft-07
- **Result**: All units conform to schema definition

### skills.json
- **Total Skills**: 8
- **Schema Version**: JSON Schema Draft-07
- **Result**: All skills conform to schema definition
- **Skill Types Validated**: DeathSpawn, DeathDamage, Shield, ChargeAttack, SplashDamage

### towers.json
- **Total Towers**: 2
- **Schema Version**: JSON Schema Draft-07
- **Result**: All towers conform to schema definition

## Notes

- All schemas have been updated to JSON Schema Draft-07 for compatibility with ajv-cli
- Wave definition schema is defined but no data file exists yet (planned for M2.3)
- Validation performed using ajv-cli v5.1.0

## Next Steps

1. Integrate validation into CI/CD pipeline
2. Add pre-commit hook for automatic validation
3. Create waves.json data file (M2.3)
