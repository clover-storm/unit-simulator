# Web-based Sim Studio Prototype

이 문서는 `sim-studio` 소스 코드를 기준으로 현재 Sim Studio 구현 구조를 요약합니다.

## 목차

- [상위 구조](#상위-구조)
- [뷰와 레이아웃](#뷰와-레이아웃)
- [Simulation](#simulation)
- [Data Editor](#data-editor)
- [유틸리티](#유틸리티)

## 상위 구조

- 엔트리: `sim-studio/src/main.tsx`가 `StrictMode` 아래에서 `sim-studio/src/App.tsx`를 렌더링합니다.
- 앱 셸: `sim-studio/src/App.tsx`가 세션 흐름, WebSocket 상태, UI 레이아웃을 총괄합니다.
- 타입: `sim-studio/src/types.ts`가 .NET 프레임/유닛 페이로드와 커맨드 메시지를 매핑합니다.
- 네트워킹: `sim-studio/src/hooks/useWebSocket.ts`가 세션 기반 WebSocket 연결, 재연결 로직, 커맨드 전송을 담당합니다.

## 뷰와 레이아웃

앱은 `App.tsx`에서 두 개의 탭을 제공합니다:

1. **Simulation** (기본)
   - 연결 전에 세션 선택 화면을 거칩니다.
   - 메인 레이아웃은 시뮬레이션 뷰와 사이드바로 구성됩니다.
2. **Data Editor**
   - REST 엔드포인트 기반의 JSON 데이터 파일 브라우저/편집기입니다.

## Simulation

### Simulation 뷰 구성 요소

- `sim-studio/src/components/SimulationCanvas.tsx`
  - 월드 그리드와 유닛을 캔버스에 팬/줌으로 렌더링합니다.
  - 유닛 선택(클릭), 패닝(드래그), 줌(휠)을 지원합니다.
  - 캔버스를 클릭하면 선택된 유닛에 이동 커맨드를 전송합니다.
  - 타겟 마커, 체력바, 방향 벡터, 선택 링을 그립니다.
  - 카메라 자동 프레이밍 스펙은 아래 섹션에 정리되어 있습니다.
- `sim-studio/src/components/UnitStateViewer.tsx`
  - 아군/적군 유닛을 체력/좌표/상태와 함께 목록으로 표시합니다.
  - 선택 강조 및 상태 플래그(이동 중, 사거리 내, 사망)를 표시합니다.
- `sim-studio/src/components/CommandPanel.tsx`
  - 선택된 유닛에 대해 이동, 체력 설정, 처치, 부활 커맨드를 전송합니다.
  - 뷰어 역할이거나 연결이 없을 때 입력을 비활성화합니다.
- `sim-studio/src/components/SimulationControls.tsx`
  - 재생/일시정지, 프레임 단위 스텝, 리셋, 탐색(seek)을 제공합니다.
  - 연결 상태 및 재생 상태와 동기화됩니다.

### 카메라 자동 프레이밍 스펙

이 문서는 `sim-studio`의 시뮬레이션 캔버스 카메라가 맵/유닛 배치에 맞춰
자동으로 줌 인/아웃 및 패닝을 수행하는 규칙을 정의합니다.

#### 목표

- 포커스 대상 유닛들이 항상 한 화면 안에 들어오도록 자동 프레이밍합니다.
- 맵 크기가 달라지더라도 일관된 카메라 스케일 동작을 보장합니다.
- 수동 조작(휠 줌/드래그 팬)을 존중하고, 자동 포커스와 충돌하지 않게 합니다.

#### 용어

- 월드 좌표: 시뮬레이션 데이터가 사용하는 실제 좌표계 (`WORLD_WIDTH`, `WORLD_HEIGHT`).
- 캔버스 좌표: 화면에 그려지는 픽셀 좌표.
- baseScale: 월드를 캔버스에 등비로 맞추기 위한 기본 스케일.
- zoom: baseScale에 추가로 곱해지는 사용자/자동 줌 배율.

#### 카메라 포커스 모드

카메라는 아래 모드 중 하나를 사용합니다. 기본값은 `auto`입니다.

- `auto`: 선택 유닛이 있으면 선택 유닛, 없으면 살아있는 전체 유닛, 그것도 없으면 전체 유닛.
- `selected`: 선택된 유닛만 포커스. 선택이 없으면 전체 유닛으로 폴백.
- `all-living`: 살아있는 유닛 전체.
- `friendly`: 아군 유닛(생존 우선, 없으면 전체 아군). 아군이 없으면 전체 유닛으로 폴백.
- `enemy`: 적군 유닛(생존 우선, 없으면 전체 적군). 적군이 없으면 전체 유닛으로 폴백.
- `all`: 전체 유닛.

##### UI 노출

- Simulation Controls 패널에 `Camera Focus` 드롭다운으로 제공됩니다.

#### 프레이밍 규칙

- 포커스 대상들의 최소 경계 박스를 계산합니다. (유닛 반지름 포함)
- 프레이밍 패딩을 월드 단위로 추가합니다. (`AUTO_FIT_PADDING`)
- 너무 과도한 확대를 막기 위해 최소 크기를 보장합니다. (`AUTO_FIT_MIN_SIZE`)
- 캔버스에 완전히 들어오도록 줌 값을 계산합니다.
  - `desiredScale = min(canvasWidth / focusWidth, canvasHeight / focusHeight)`
  - `zoom = clamp(desiredScale / baseScale, MIN_ZOOM, MAX_ZOOM)`
- 포커스 박스 중심이 캔버스 중심에 오도록 팬 값을 계산합니다.

#### 수동 조작 우선 규칙

- 사용자가 휠 줌 또는 드래그 팬을 수행하면 일정 시간 동안 자동 프레이밍을 중단합니다.
- 중단 시간(`AUTO_FIT_COOLDOWN_MS`) 이후 자동 프레이밍이 다시 활성화됩니다.
- 선택 유닛이 변경되면 자동 프레이밍을 즉시 재평가합니다.

#### 기본 파라미터

- `AUTO_FIT_PADDING`: 60 (월드 단위)
- `AUTO_FIT_MIN_SIZE`: 200 (월드 단위)
- `AUTO_FIT_COOLDOWN_MS`: 2000ms
- `CAMERA_PAN_SPEED`: 1600 (캔버스 픽셀/초)
- `CAMERA_ZOOM_SPEED`: 1.5 (줌 배율/초)

#### 카메라 애니메이션 규칙

- 카메라 이동(패닝)과 줌은 일정 속도로 보간되며, 목표 값에 근접하면 스냅합니다.
- 자동 프레이밍과 수동 조작 모두 동일한 애니메이션 규칙을 사용합니다.

#### 향후 확장 포인트

- UI 토글로 자동 프레이밍 On/Off 제어
- 특정 그룹(아군만, 적군만, 전투 중 유닛만 등) 선택 옵션
- 포커스 대상 변경 시 부드러운 보간(lerp) 애니메이션

### 세션 흐름

- `sim-studio/src/components/SessionSelector.tsx`
  - `/sessions`에서 세션 목록을 가져와 생성/참가를 지원합니다.
  - 세션 상태, 프레임, 클라이언트 수, 마지막 활동 시간을 표시합니다.
  - 오너 연결 해제 시 읽기 전용 경고를 표시합니다.
- `useWebSocket.ts`는 `ws://localhost:5000/ws/{sessionId|new}`에 연결합니다.
  - 지속적인 client id로 identify 메시지를 전송합니다.
  - 세션 역할(`owner` vs `viewer`)과 오너 연결 상태를 추적합니다.
  - 다운로드/탐색을 위한 프레임 로그를 유지합니다.
  - 지수 백오프로 자동 재연결합니다.

### 런타임 상호작용

- 키보드 단축키: 좌/우 화살표로 이전/다음 프레임 이동.
- 권한: 세션 오너만 시뮬레이션 제어/커맨드 전송이 가능합니다.

## Data Editor

- `sim-studio/src/components/DataEditor.tsx`
  - `/data/files`에서 파일 목록을 불러옵니다.
  - `/data/file?path=...`로 로드/저장/삭제합니다.
  - 원시 JSON 편집과 배열 레코드 단위 편집을 모두 지원합니다.
  - ETag로 저장 충돌을 감지합니다.

### 스프레드시트 뷰 (구현 완료)

- `sim-studio/src/components/SpreadsheetView.tsx`
  - AG Grid Community 기반의 엑셀 스타일 데이터 편집기입니다.
  - 셀 더블클릭 또는 타이핑으로 인라인 편집이 가능합니다.
  - 입력값의 자동 타입 변환을 지원합니다 (숫자, boolean, JSON 객체/배열).
  - 열 헤더 클릭으로 정렬, 열 경계 드래그로 너비 조정이 가능합니다.
  - Ctrl+Z/Y로 Undo/Redo를 지원합니다 (최대 20단계).
  - 다크 테마가 적용되어 기존 UI와 일관성을 유지합니다.

### 리사이저 (구현 완료)

- `sim-studio/src/components/Resizer.tsx`
  - 스프레드시트 뷰 하단에 드래그 핸들을 제공합니다.
  - 세로 드래그로 스프레드시트 높이를 조절합니다 (150px ~ 800px).
  - 호버/드래그 시 시각적 피드백을 제공합니다.

### 파일명 변경 (구현 완료)

- 파일 목록에서 파일명 옆 연필 아이콘으로 이름 변경이 가능합니다.
- 인라인 편집 모드에서 Enter로 확정, Escape로 취소합니다.
- 내부적으로 파일 복사 후 원본 삭제 방식으로 동작합니다.

### 뷰 모드

Data Editor는 세 가지 뷰 모드를 탭으로 전환할 수 있습니다:

| 모드 | 설명 |
| --- | --- |
| **Spreadsheet** | AG Grid 기반 엑셀 스타일 편집 (기본값) |
| **Table** | 기본 HTML 테이블 뷰 (검색/필터/정렬) |
| **Raw JSON** | 원시 JSON 텍스트 편집 |

### 개선 계획

목표는 "JSON 원본" 중심의 단순 편집기를 "테이블 기반 데이터 브라우저 + 규약 기반 편집기"로 전환하는 것입니다.

- 데이터 조회 우선 UX
  - 기본 화면은 Excel 형태의 테이블 그리드로 표시합니다.
  - 행 선택 시 사이드 패널에서 상세 필드 편집을 제공합니다.
  - 빠른 탐색을 위해 검색/필터/정렬을 기본 제공하며, 최근 수정/즐겨찾기 필터를 포함합니다.
- 레코드 편집 흐름
  - 테이블에서 레코드를 선택 -> 필드 편집 -> 즉시 검증 -> 저장 큐에 추가.
  - 변경 사항은 "미저장 배지"와 변경 내역(diff)로 표시합니다.
  - 대량 수정은 다중 선택 + 일괄 편집 UI로 처리합니다.
- 스키마/규약 정의 (스크립트 영역)
  - "규약 스크립트"를 별도 탭 또는 파일로 제공하여 프로그래머/기획자가 정의합니다.
  - 스키마는 테이블 단위와 필드 단위로 구분합니다.
  - 예시 규약 (개념):
    - 테이블명/설명, 기본 정렬 키, 표시 컬럼
    - 필드 타입 (string, number, boolean, enum, vector 등)
    - 필드 제약 (required, min/max, unique, regex)
    - 읽기 전용/계산식 필드, 기본값, 표시 포맷
- 입력 제한 및 검증
  - 규약에 따라 허용되지 않는 값은 입력 단계에서 차단합니다.
  - 저장 전/후 검증 결과를 표/필드 단위로 보여줍니다.
  - 규약 미정의 필드는 "알 수 없음"으로 표시하고 편집 제한을 걸 수 있습니다.
- 데이터 모델 및 저장
  - 실제 저장 대상은 JSON이지만, 테이블 뷰는 스키마에 맞춰 배열/객체를 평탄화해 보여줍니다.
  - 저장 시 스키마 기반으로 원본 JSON을 재구성합니다.
  - ETag 충돌 시 재로딩/머지 안내를 제공합니다.

### 규약 스크립트

#### 결정 항목 (협의 필요)

아래 항목은 규약 스크립트 설계를 위해 확정이 필요한 질문 목록입니다. 각 항목은 결정 전/후 상태를 관리합니다.

| 항목 | 질문 | 상태 | 결정 |
| --- | --- | --- | --- |
| 형식 | `JSON Schema`, `Custom DSL`, `TypeScript config`, `YAML` 중 선호가 있나요? | 결정 | YAML 기반 JSON Schema + `x-ui` 확장 |
| 위치 | 규약 파일 위치는 어디가 적절한가요? (예: `sim-studio/public/schema`, 서버 데이터 폴더, JSON 옆 `*.schema.*`) | 결정 | `sim-studio/config/schema` (git 관리) |
| 버전 | 스키마 버전 관리는 어떻게 하나요? (파일별/전역/마이그레이션 규칙) | 결정 | git 이력으로 관리, 별도 버저닝 불필요 |
| 검증 타이밍 | 입력 즉시/저장 직전/둘 다 중 무엇을 기본으로 할까요? | 결정 | 저장 시에만 검증 |
| 커스텀 타입 | `Vector2`, `Color`, `Range`, `Enum`, `Tags` 등 도메인 타입을 지원할까요? | 결정 | 지원함 (세부 목록/표현은 추후 확정) |
| 계산/읽기 전용 | 계산 필드 또는 읽기 전용 필드가 필요한가요? | 결정 | 읽기 전용 필드 필요 (예: uid는 자동 생성, 수정 불가) |
| 참조 규칙 | 다른 테이블 참조(외래키 유사 제약)가 필요한가요? | 결정 | uid-string 기반 참조는 허용, 외래키 강제 규약은 없음(로더가 처리) |
| 표시 포맷 | 단위/소수점/포맷 규칙이 필요한가요? | 결정 | 커스텀 타입 기반 포맷 지원, `|`, `[]` 토큰으로 페어/구분자/배열 표현 |
| 권한/역할 | 역할별 편집 제한이 필요한가요? | 결정 | 별도 권한 없음, 접근 가능하면 누구나 수정 |
| 편집자 | 비개발자도 규약을 수정해야 하나요? | 결정 | 가능 (YAML로 직접 편집) |

#### 결정 로그 (단계별 관리)

이 섹션은 결정된 항목과 현재 상태를 단계적으로 기록합니다. 결정이 날 때마다 상태를 갱신합니다.

| 단계 | 범위 | 결정 상태 | 결정 내용 | 비고 |
| --- | --- | --- | --- | --- |
| 1 | 규약 형식/위치 | 결정 | 형식: YAML 기반 JSON Schema + `x-ui` 확장 | 위치: `sim-studio/config/schema` (git 관리) |
| 2 | 버전 관리 | 결정 | git 이력으로 관리, 별도 버저닝 불필요 | - |
| 3 | 검증 타이밍 | 결정 | 저장 시에만 검증 | - |
| 4 | 커스텀 타입 | 결정 | 커스텀 타입 지원 (세부 목록/표현 추후 확정) | - |
| 5 | 읽기 전용/계산 필드 | 결정 | uid 등 자동 생성 값은 수정 불가 | - |
| 6 | 참조 규칙 | 결정 | uid-string 기반 참조 허용, 외래키 강제 규약 없음 | 로더 처리 |
| 7 | UI 편집 규칙/포맷 | 결정 | 커스텀 타입별 포맷, `|`, `[]` 토큰 기반 표현 | - |
| 8 | 편집자/워크플로우 | 결정 | 비개발자도 규약 수정 가능 (YAML 직접 편집) | - |
| 9 | 권한/워크플로우 | 결정 | 별도 권한 없음, 접근 가능하면 누구나 수정 | - |
| 10 | 타입/검증 규칙 | 진행 중 | 엔지니어 선규약 범위 내에서만 조정 | 세부 규칙 협의 필요 |
| 4 | UI 편집 규칙/포맷 | 대기 | - | - |
| 5 | 권한/워크플로우 | 대기 | - | - |

#### 스펙 (확정)

현재 확정된 규약 스크립트 스펙은 다음과 같습니다.

- 파일 형식: YAML
- 기반 규격: JSON Schema
- UI 확장: 스키마에 `x-ui` 메타데이터를 추가하여 컬럼/라벨/표시 포맷을 정의
- 파일 위치: `sim-studio/config/schema` (git 관리)
- 버전 관리: git 이력으로 관리, 별도 버저닝 불필요
- 검증 타이밍: 저장 시에만 검증
- 커스텀 타입: 필요 (세부 목록/표현은 추후 확정)
- 읽기 전용 필드: uid 등 자동 생성 값은 수정 불가
- 참조 규칙: uid-string 기반 참조 허용, 외래키 강제 규약 없음 (로더 처리)
- 표시 포맷: 커스텀 타입별 포맷 지원, `|`, `[]` 토큰으로 페어/구분자/배열 표현
- 권한: 별도 권한 없음, 접근 가능하면 누구나 수정
- 편집자: 비개발자도 규약 수정 가능 (YAML 직접 편집)
- 타입/검증 규칙: 엔지니어 선규약 범위 내에서만 조정

#### 스펙 (초안 예시)

아래는 기본 문법 확인을 위한 초안입니다. 실제 규약은 추후 협의로 확정합니다.

```yaml
$schema: "https://json-schema.org/draft/2020-12/schema"
title: "Unit Data"
type: "array"
items:
  type: "object"
  required: ["id", "name", "hp", "speed", "position"]
  properties:
    id:
      type: "integer"
      minimum: 1
      x-ui:
        label: "ID"
        column: true
        width: 80
    name:
      type: "string"
      minLength: 1
      x-ui:
        label: "Name"
        column: true
        width: 160
    hp:
      type: "integer"
      minimum: 0
      maximum: 100
      x-ui:
        label: "HP"
        column: true
        format: "percent-0-100"
    speed:
      type: "number"
      minimum: 0
      maximum: 20
      x-ui:
        label: "Speed"
        column: true
        format: "float-1"
    position:
      type: "object"
      required: ["x", "y"]
      properties:
        x:
          type: "number"
          minimum: 0
          maximum: 1200
          x-ui:
            label: "Pos X"
            column: true
            format: "float-1"
        y:
          type: "number"
          minimum: 0
          maximum: 720
          x-ui:
            label: "Pos Y"
            column: true
            format: "float-1"
    isActive:
      type: "boolean"
      default: true
      x-ui:
        label: "Active"
        column: true
    tag:
      type: ["string", "null"]
      x-ui:
        label: "Tag"
        column: false
```

##### 초안에서 다루는 규약 범위

- sign/unsign: `minimum`/`maximum`과 `integer`/`number`로 표현
- range: `minimum`/`maximum`
- nullable: `type`에 `"null"` 포함

이 초안을 기준으로 enum, regex, unique, 참조 규칙 등은 다음 단계에서 협의합니다.

#### x-ui 메타데이터 초안 스펙 (제안)

아래는 Data Editor의 테이블/폼 렌더링을 위한 `x-ui` 메타데이터 초안입니다. 필요에 따라 수정합니다.

##### 공통 필드 (모든 속성에서 사용 가능)

- `label`: 표시 이름
- `column`: 테이블 컬럼 노출 여부 (boolean)
- `order`: 컬럼/필드 순서 (number)
- `width`: 컬럼 너비 (px)
- `help`: 도움말 텍스트
- `readonly`: 편집 불가 (uid 등 자동 생성 필드)
- `placeholder`: 입력 플레이스홀더

##### 입력 위젯 지정

- `editor`: `"text" | "number" | "toggle" | "select" | "textarea" | "vector2" | "list" | "pair"`
- `options`: `select`용 옵션 리스트 (string 배열 또는 `{ label, value }[]`)

##### 표시/포맷 지정

- `format`: 표시 문자열 규칙 (예: `"float-1"`, `"int"`, `"percent-0-100"`)
- `separator`: 배열 표시 구분자 (기본: `","`)
- `pairToken`: 페어 표시 토큰 (기본: `"|"`)
- `arrayToken`: 배열 표시 토큰 (기본: `"[]"`)
- `display`: `"inline" | "block"`

##### 예시

```yaml
properties:
  uid:
    type: "string"
    x-ui:
      label: "UID"
      column: true
      readonly: true
      width: 180
  position:
    type: "object"
    properties:
      x: { type: "number" }
      y: { type: "number" }
    x-ui:
      label: "Position"
      column: true
      editor: "vector2"
      format: "float-1"
      pairToken: "|"
  tags:
    type: "array"
    items: { type: "string" }
    x-ui:
      label: "Tags"
      column: false
      editor: "list"
      separator: ","
      arrayToken: "[]"
```

#### 커스텀 타입 초안 목록 (제안)

Data Editor에서 자주 쓰일 것으로 보이는 도메인 타입을 우선 제안합니다. 실제 채택 여부는 추후 조정합니다.

- `uid-string`
  - 설명: UID 문자열 (예: CRC 기반 자동 생성)
  - 편집: `readonly: true`
  - 저장 검증: 문자열 길이/패턴 체크 가능
- `vector2`
  - 설명: 2차원 좌표 (x, y)
  - 편집: `editor: "vector2"`, `pairToken: "|"`
  - 표시: `x|y` 또는 `x|y[]` (배열일 경우)
- `range`
  - 설명: 구간 값 (min, max)
  - 편집: `editor: "pair"` 또는 별도 2칸 입력
  - 표시: `min|max`
- `enum`
  - 설명: 열거형 문자열/숫자
  - 편집: `editor: "select"`, `options` 사용
- `weighted-list`
  - 설명: 아이템과 가중치 쌍 리스트
  - 편집: `editor: "list"`, 아이템은 `value|weight` 형식
  - 표시: `value|weight, value|weight`
- `tags`
  - 설명: 문자열 배열 (태그)
  - 편집: `editor: "list"`, `separator: ","`
  - 표시: `tag1, tag2`
- `color`
  - 설명: 색상 (hex 또는 rgba)
  - 편집: `editor: "text"` 또는 color picker 대응
  - 표시: `#RRGGBB` 또는 `rgba(...)`
- `angle`
  - 설명: 각도 값 (도/라디안)
  - 편집: `editor: "number"`
  - 표시: `deg` 또는 `rad` 포맷 지원

##### 커스텀 타입 표기 메타데이터 (제안)

스키마의 `x-ui` 또는 `x-type`에 커스텀 타입 명시:

```yaml
properties:
  uid:
    type: "string"
    x-type: "uid-string"
    x-ui:
      label: "UID"
      readonly: true
  position:
    type: "object"
    x-type: "vector2"
    x-ui:
      label: "Position"
      editor: "vector2"
```

## 유틸리티

- `sim-studio/src/utils/frameLogDownload.ts`
  - 수집된 프레임 로그를 JSON 파일로 다운로드합니다.
