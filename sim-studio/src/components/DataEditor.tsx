import { useCallback, useEffect, useMemo, useState } from 'react';

type DataFile = {
  path: string;
  size: number;
  modifiedUtc: string;
  etag: string;
};

type FileResponse = {
  path: string;
  content: string;
  etag: string;
  modifiedUtc: string;
};

type FilesResponse = {
  root: string;
  files: DataFile[];
};

type Props = {
  apiBaseUrl: string;
};

function formatBytes(bytes: number) {
  if (bytes < 1024) return `${bytes} B`;
  const kb = bytes / 1024;
  if (kb < 1024) return `${kb.toFixed(1)} KB`;
  return `${(kb / 1024).toFixed(1)} MB`;
}

function recordLabel(record: unknown, index: number) {
  if (record && typeof record === 'object') {
    const candidate = record as Record<string, unknown>;
    const labelKey = ['id', 'name', 'key', 'role', 'type'].find(k => typeof candidate[k] === 'string' || typeof candidate[k] === 'number');
    if (labelKey) {
      return `${index} · ${candidate[labelKey]}`;
    }
  }
  return `${index}`;
}

export default function DataEditor({ apiBaseUrl }: Props) {
  const [files, setFiles] = useState<DataFile[]>([]);
  const [selectedPath, setSelectedPath] = useState<string | null>(null);
  const [documentValue, setDocumentValue] = useState<unknown>(null);
  const [rawDraft, setRawDraft] = useState('');
  const [etag, setEtag] = useState<string | null>(null);
  const [modifiedUtc, setModifiedUtc] = useState<string | null>(null);
  const [selectedIndex, setSelectedIndex] = useState<number | null>(null);
  const [recordDraft, setRecordDraft] = useState('');
  const [newFilePath, setNewFilePath] = useState('new-data.json');
  const [status, setStatus] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(false);

  const isArray = useMemo(() => Array.isArray(documentValue), [documentValue]);

  const loadFiles = useCallback(async () => {
    setError(null);
    try {
      const res = await fetch(`${apiBaseUrl}/data/files`);
      if (!res.ok) throw new Error('Failed to load file list.');
      const data = (await res.json()) as FilesResponse;
      setFiles(data.files);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Unknown error.');
    }
  }, [apiBaseUrl]);

  const loadFile = useCallback(async (path: string) => {
    setError(null);
    setStatus(null);
    setIsLoading(true);
    try {
      const res = await fetch(`${apiBaseUrl}/data/file?path=${encodeURIComponent(path)}`);
      if (!res.ok) throw new Error('Failed to load file.');
      const data = (await res.json()) as FileResponse;
      const parsed = JSON.parse(data.content);
      setSelectedPath(path);
      setDocumentValue(parsed);
      setRawDraft(JSON.stringify(parsed, null, 2));
      setEtag(data.etag);
      setModifiedUtc(data.modifiedUtc);
      if (Array.isArray(parsed) && parsed.length > 0) {
        setSelectedIndex(0);
        setRecordDraft(JSON.stringify(parsed[0], null, 2));
      } else {
        setSelectedIndex(null);
        setRecordDraft('');
      }
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Unknown error.');
    } finally {
      setIsLoading(false);
    }
  }, [apiBaseUrl]);

  const applyRawDraft = useCallback(() => {
    try {
      const parsed = JSON.parse(rawDraft);
      setDocumentValue(parsed);
      setStatus('Raw JSON applied.');
      setError(null);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Invalid JSON.');
    }
  }, [rawDraft]);

  const applyRecordDraft = useCallback(() => {
    if (!Array.isArray(documentValue) || selectedIndex === null) return;
    try {
      const parsed = JSON.parse(recordDraft);
      const next = [...documentValue];
      next[selectedIndex] = parsed;
      setDocumentValue(next);
      setRawDraft(JSON.stringify(next, null, 2));
      setStatus('Record updated.');
      setError(null);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Invalid record JSON.');
    }
  }, [documentValue, recordDraft, selectedIndex]);

  const addRecord = useCallback(() => {
    if (!Array.isArray(documentValue)) return;
    const next = [...documentValue, {}];
    const newIndex = next.length - 1;
    setDocumentValue(next);
    setRawDraft(JSON.stringify(next, null, 2));
    setSelectedIndex(newIndex);
    setRecordDraft(JSON.stringify(next[newIndex], null, 2));
    setStatus('Record added.');
  }, [documentValue]);

  const deleteRecord = useCallback(() => {
    if (!Array.isArray(documentValue) || selectedIndex === null) return;
    const next = [...documentValue];
    next.splice(selectedIndex, 1);
    setDocumentValue(next);
    setRawDraft(JSON.stringify(next, null, 2));
    setSelectedIndex(next.length ? Math.min(selectedIndex, next.length - 1) : null);
    setRecordDraft(next.length ? JSON.stringify(next[Math.min(selectedIndex, next.length - 1)], null, 2) : '');
    setStatus('Record removed.');
  }, [documentValue, selectedIndex]);

  const saveFile = useCallback(async () => {
    if (!selectedPath) return;
    setError(null);
    setStatus(null);
    const content = JSON.stringify(documentValue, null, 2);
    try {
      const res = await fetch(`${apiBaseUrl}/data/file?path=${encodeURIComponent(selectedPath)}`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ content, etag })
      });
      if (res.status === 409) {
        const data = await res.json();
        setError('Conflict detected. Reload before saving.');
        setEtag(data.etag ?? null);
        return;
      }
      if (!res.ok) throw new Error('Failed to save file.');
      const data = await res.json();
      setEtag(data.etag ?? null);
      setStatus('Saved.');
      loadFiles();
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Unknown error.');
    }
  }, [apiBaseUrl, documentValue, etag, loadFiles, selectedPath]);

  const createFile = useCallback(async () => {
    if (!newFilePath.trim()) return;
    setError(null);
    try {
      const res = await fetch(`${apiBaseUrl}/data/file?path=${encodeURIComponent(newFilePath)}`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ content: '[]' })
      });
      if (!res.ok) throw new Error('Failed to create file.');
      await loadFiles();
      await loadFile(newFilePath);
      setStatus('File created.');
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Unknown error.');
    }
  }, [apiBaseUrl, loadFile, loadFiles, newFilePath]);

  const deleteFile = useCallback(async () => {
    if (!selectedPath) return;
    if (!window.confirm(`Delete ${selectedPath}?`)) return;
    setError(null);
    try {
      const res = await fetch(`${apiBaseUrl}/data/file?path=${encodeURIComponent(selectedPath)}`, {
        method: 'DELETE'
      });
      if (!res.ok) throw new Error('Failed to delete file.');
      setSelectedPath(null);
      setDocumentValue(null);
      setRawDraft('');
      setSelectedIndex(null);
      setRecordDraft('');
      await loadFiles();
      setStatus('File deleted.');
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Unknown error.');
    }
  }, [apiBaseUrl, loadFiles, selectedPath]);

  useEffect(() => {
    loadFiles();
  }, [loadFiles]);

  useEffect(() => {
    if (Array.isArray(documentValue) && selectedIndex !== null) {
      setRecordDraft(JSON.stringify(documentValue[selectedIndex], null, 2));
    }
  }, [documentValue, selectedIndex]);

  return (
    <div className="data-editor">
      <aside className="panel data-editor-sidebar">
        <h2>Data Files</h2>
        <div className="data-editor-create">
          <input
            type="text"
            value={newFilePath}
            onChange={e => setNewFilePath(e.target.value)}
            placeholder="new-data.json"
          />
          <button className="btn-primary" onClick={createFile}>Create</button>
        </div>
        <div className="data-editor-file-list">
          {files.map(file => (
            <button
              key={file.path}
              className={`data-file ${file.path === selectedPath ? 'active' : ''}`}
              onClick={() => loadFile(file.path)}
            >
              <div className="data-file-name">{file.path}</div>
              <div className="data-file-meta">{formatBytes(file.size)}</div>
            </button>
          ))}
        </div>
      </aside>

      <section className="panel data-editor-main">
        <div className="data-editor-header">
          <div>
            <h2>Editor</h2>
            {selectedPath && (
              <div className="data-editor-subtitle">
                {selectedPath}
                {modifiedUtc && <span> · {new Date(modifiedUtc).toLocaleString()}</span>}
              </div>
            )}
          </div>
          <div className="data-editor-actions">
            <button className="btn-secondary" onClick={loadFiles}>Refresh</button>
            <button className="btn-secondary" onClick={deleteFile} disabled={!selectedPath}>Delete File</button>
            <button className="btn-primary" onClick={saveFile} disabled={!selectedPath}>Save</button>
          </div>
        </div>

        {error && <div className="data-editor-error">{error}</div>}
        {status && <div className="data-editor-status">{status}</div>}
        {isLoading && <div className="data-editor-status">Loading...</div>}

        {!selectedPath && (
          <div className="data-editor-empty">Select a file to edit.</div>
        )}

        {selectedPath && (
          <div className="data-editor-body">
            {isArray && (
              <div className="data-editor-records">
                <div className="data-editor-record-list">
                  <div className="data-editor-record-header">
                    <h3>Records</h3>
                    <div className="data-editor-record-actions">
                      <button className="btn-secondary" onClick={addRecord}>Add</button>
                      <button className="btn-secondary" onClick={deleteRecord} disabled={selectedIndex === null}>Remove</button>
                    </div>
                  </div>
                  <div className="data-editor-records-scroll">
                    {documentValue.map((record, index) => (
                      <button
                        key={index}
                        className={`data-record ${selectedIndex === index ? 'active' : ''}`}
                        onClick={() => setSelectedIndex(index)}
                      >
                        {recordLabel(record, index)}
                      </button>
                    ))}
                  </div>
                </div>
                <div className="data-editor-record-editor">
                  <h3>Record JSON</h3>
                  <textarea
                    value={recordDraft}
                    onChange={e => setRecordDraft(e.target.value)}
                    rows={14}
                  />
                  <button className="btn-primary" onClick={applyRecordDraft} disabled={selectedIndex === null}>
                    Apply Record
                  </button>
                </div>
              </div>
            )}

            <div className="data-editor-raw">
              <h3>Raw JSON</h3>
              <textarea
                value={rawDraft}
                onChange={e => setRawDraft(e.target.value)}
                rows={16}
              />
              <button className="btn-secondary" onClick={applyRawDraft}>
                Apply Raw JSON
              </button>
            </div>
          </div>
        )}
      </section>
    </div>
  );
}
