import { useCallback, useEffect, useState } from 'react';
import Ajv, { ErrorObject } from 'ajv';
import addFormats from 'ajv-formats';

type SchemaInfo = {
  name: string;
  path: string;
};

type SchemaMapping = Record<string, string>;

// Map data file names to schema names
const FILE_TO_SCHEMA: SchemaMapping = {
  'units.json': 'unit-stats',
  'skills.json': 'skill-reference',
  'towers.json': 'tower-reference',
  'waves.json': 'wave-definition',
  'balance.json': 'game-balance',
};

export type ValidationResult = {
  valid: boolean;
  errors: ErrorObject[] | null;
  errorMessages: string[];
};

export function useSchemaValidator(apiBaseUrl: string) {
  const [schemas, setSchemas] = useState<SchemaInfo[]>([]);
  const [ajv, setAjv] = useState<Ajv | null>(null);
  const [loadedSchemas, setLoadedSchemas] = useState<Set<string>>(new Set());
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // Initialize Ajv instance
  useEffect(() => {
    const ajvInstance = new Ajv({
      allErrors: true,
      verbose: true,
      strict: false,
    });
    addFormats(ajvInstance);
    setAjv(ajvInstance);
  }, []);

  // Load schema list
  const loadSchemaList = useCallback(async () => {
    try {
      const res = await fetch(`${apiBaseUrl}/data/schemas`);
      if (!res.ok) throw new Error('Failed to load schema list');
      const data = await res.json();
      setSchemas(data.schemas || []);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load schemas');
    }
  }, [apiBaseUrl]);

  // Load and compile a specific schema
  const loadSchema = useCallback(async (schemaName: string): Promise<boolean> => {
    if (!ajv || loadedSchemas.has(schemaName)) return true;

    setIsLoading(true);
    try {
      const res = await fetch(`${apiBaseUrl}/data/schema?name=${encodeURIComponent(schemaName)}`);
      if (!res.ok) {
        console.warn(`Schema '${schemaName}' not found`);
        return false;
      }

      const data = await res.json();
      ajv.addSchema(data.schema, schemaName);
      setLoadedSchemas(prev => new Set(prev).add(schemaName));
      return true;
    } catch (err) {
      console.error(`Failed to load schema '${schemaName}':`, err);
      return false;
    } finally {
      setIsLoading(false);
    }
  }, [ajv, apiBaseUrl, loadedSchemas]);

  // Get schema name for a data file
  const getSchemaForFile = useCallback((filePath: string): string | null => {
    const fileName = filePath.split('/').pop() || '';
    return FILE_TO_SCHEMA[fileName] || null;
  }, []);

  // Validate data against a schema
  const validate = useCallback(async (
    filePath: string,
    data: unknown
  ): Promise<ValidationResult> => {
    if (!ajv) {
      return { valid: false, errors: null, errorMessages: ['Validator not initialized'] };
    }

    const schemaName = getSchemaForFile(filePath);
    if (!schemaName) {
      // No schema for this file, consider it valid
      return { valid: true, errors: null, errorMessages: [] };
    }

    // Load schema if not already loaded
    const loaded = await loadSchema(schemaName);
    if (!loaded) {
      // Schema not available, consider it valid (warn only)
      return { valid: true, errors: null, errorMessages: [`Warning: No schema '${schemaName}' found`] };
    }

    const validateFn = ajv.getSchema(schemaName);
    if (!validateFn) {
      return { valid: true, errors: null, errorMessages: [`Warning: Schema '${schemaName}' not compiled`] };
    }

    const valid = validateFn(data);
    if (valid) {
      return { valid: true, errors: null, errorMessages: [] };
    }

    const errors = validateFn.errors || [];
    const errorMessages = errors.map(err => {
      const path = err.instancePath || '/';
      const message = err.message || 'Unknown error';
      return `${path}: ${message}`;
    });

    return { valid: false, errors, errorMessages };
  }, [ajv, getSchemaForFile, loadSchema]);

  // Quick check if a file has a schema
  const hasSchema = useCallback((filePath: string): boolean => {
    return getSchemaForFile(filePath) !== null;
  }, [getSchemaForFile]);

  // Load schema list on mount
  useEffect(() => {
    loadSchemaList();
  }, [loadSchemaList]);

  return {
    schemas,
    isLoading,
    error,
    validate,
    hasSchema,
    getSchemaForFile,
    loadSchemaList,
  };
}
