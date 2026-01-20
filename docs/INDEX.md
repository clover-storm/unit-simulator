# 📚 Documentation Index

> 단일 진실 원천(Single Source of Truth) - 모든 문서의 시작점

---

## 🎯 Quick Start (처음 오신 분들)

1. **프로젝트 이해**: [README.md](../README.md)
2. **에이전트 규칙**: [AGENTS.md](../AGENTS.md) (자동 주입됨)
3. **현재 상태**: [development-milestone.md](development-milestone.md)

---

## 📂 문서 분류 체계

### 문서 유형 (Document Types)

| 유형 | 접두사 | 설명 | 예시 |
|------|--------|------|------|
| **ADR** | `adr-` | Architecture Decision Record | 아키텍처 결정 기록 |
| **SPEC** | `-spec` | Specification | 시스템/기능 명세 |
| **GUIDE** | `-guide` | How-to Guide | 개발 가이드 |
| **PLAN** | `-plan` | Planning Document | 계획/로드맵 |
| **SUMMARY** | `-summary` | Work Summary | 작업 결과 요약 |
| **CONTEXT** | `-context` | Context Document | 세션 컨텍스트 공유용 |

---

## 📂 문서 분류

### 🏗️ 아키텍처 & 설계 (ADR/SPEC)

| 문서 | 유형 | 설명 | 상태 |
|------|------|------|------|
| [development-milestone.md](development-milestone.md) | PLAN | 전체 로드맵 및 Phase별 작업 계획 | ✅ 최신 |
| [architecture/unity-integration-design.md](architecture/unity-integration-design.md) | ADR | Unity 통합 인터페이스 설계 | 🆕 신규 |
| [simulation-spec.md](simulation-spec.md) | SPEC | 게임 로직 상세 스펙 | ✅ 최신 |
| [development-guide.md](development-guide.md) | GUIDE | 개발 인프라, WebSocket 서버 가이드 | ✅ 최신 |
| [unit-system-spec.md](unit-system-spec.md) | SPEC | 유닛 시스템 설계 | ✅ 최신 |
| [TOWER_SYSTEM_CONTEXT.md](TOWER_SYSTEM_CONTEXT.md) | CONTEXT | 타워 스킬 시스템 컨텍스트 | ✅ 최신 |

### 🤖 에이전트 관련

| 문서 | 설명 | 상태 |
|------|------|------|
| [AGENTS.md](../AGENTS.md) | 에이전트 협업 규칙 (루트) | ✅ 자동 주입 |
| [agent-quick-start.md](agent-quick-start.md) | 에이전트 빠른 시작 가이드 | ⚠️ 검토 필요 |
| [agent-integration-plan.md](agent-integration-plan.md) | 에이전트 통합 계획 | 📋 계획 |
| [skill-ownership-analysis.md](skill-ownership-analysis.md) | 스킬 소유권 분석 | 📋 참고 |

### 📊 데이터 & 레퍼런스

| 문서 | 설명 | 상태 |
|------|------|------|
| [data/schemas/README.md](../data/schemas/README.md) | JSON Schema 가이드 | ✅ 최신 |
| [ReferenceModels/README.md](../ReferenceModels/README.md) | 데이터 모델 매핑 규칙 | ✅ 최신 |
| [reference-models-expansion-plan.md](reference-models-expansion-plan.md) | 확장 계획 | 📋 계획 |

### 🛠️ 개발 도구

| 문서 | 설명 | 상태 |
|------|------|------|
| [sim-studio.md](sim-studio.md) | 웹 기반 시뮬레이션 스튜디오 | ✅ 최신 |
| [session-logging.md](session-logging.md) | WebSocket 세션 로깅 | ✅ 최신 |
| [multi-session-spec.md](multi-session-spec.md) | 멀티 세션 스펙 | ✅ 최신 |

### 📝 작업 요약 (완료된 작업들)

| 문서 | 설명 | 상태 |
|------|------|------|
| [claude-md-completion-summary.md](claude-md-completion-summary.md) | CLAUDE.md 완성 요약 | ✅ 완료 |
| [phase1-documenter-extension-summary.md](phase1-documenter-extension-summary.md) | Documenter 확장 요약 | ✅ 완료 |
| [skill-reallocation-summary.md](skill-reallocation-summary.md) | 스킬 재할당 요약 | ✅ 완료 |

---

## 🎯 작업별 참조 가이드

### 새 기능 개발 시
1. [development-milestone.md](development-milestone.md) - 현재 Phase 확인
2. [AGENTS.md](../AGENTS.md) - 에이전트 역할 확인
3. [simulation-spec.md](simulation-spec.md) - 게임 로직 스펙

### 데이터 수정 시
1. [data/schemas/README.md](../data/schemas/README.md) - 스키마 규칙
2. `npm run data:validate` - 검증 실행

### WebSocket API 개발 시
1. [development-guide.md](development-guide.md) - 서버 구조
2. [session-logging.md](session-logging.md) - 로깅 규칙

### 테스트 작성 시
1. [development-guide.md](development-guide.md) - 테스트 가이드
2. `dotnet test` - 테스트 실행

---

## 📌 현재 프로젝트 상태 (2026-01-20)

| Phase | 상태 | 진행률 |
|-------|------|--------|
| Phase 1: 코어 분리 및 안정화 | ✅ 완료 | 100% |
| Phase 2.1: 데이터 스키마 표준화 | ✅ 완료 | 100% |
| Phase 2.2: 데이터 변환 파이프라인 | ✅ 완료 | 100% |
| Phase 2.3: 런타임 데이터 로더 | ✅ 완료 | 100% |
| Phase 2.4: 데이터 뷰/에디터 | ✅ 완료 | 100% |
| Phase 3: 게임 엔진 통합 | 🚧 설계 중 | 10% |

---

## 🔄 문서 업데이트 정책

### 필수 업데이트 대상
- `development-milestone.md` - Phase 완료 시
- `AGENTS.md` - 에이전트 역할 변경 시
- `data/schemas/README.md` - 스키마 추가/변경 시

### 자동 업데이트
- `CHANGELOG.md` - Documenter 에이전트가 자동 생성
- `data/validation/report.md` - npm run data:validate 실행 시

---

## ❓ FAQ

**Q: 어떤 문서부터 읽어야 하나요?**
A: README.md → AGENTS.md → development-milestone.md 순서로 읽으세요.

**Q: 문서가 너무 많은데 어떻게 관리하나요?**
A: 이 INDEX.md를 시작점으로 사용하고, 작업별로 필요한 문서만 참조하세요.

**Q: 문서가 최신인지 어떻게 확인하나요?**
A: 각 문서의 "변경 이력" 섹션 또는 git log를 확인하세요.

**Q: 새 문서는 어디에 만들어야 하나요?**
A: 문서 유형에 따라:
- `docs/architecture/` - ADR, 아키텍처 설계
- `docs/specs/` - SPEC, 시스템 명세
- `docs/guides/` - GUIDE, 개발 가이드
- `docs/summaries/` - SUMMARY, 작업 요약
- `docs/sessions/` - CONTEXT, 세션 컨텍스트 (임시)

---

## 🔧 세션/컨텍스트 관리

### 긴 논의를 문서로 저장하는 방법

세션이 길어지면 논의 내용을 문서로 정리하여:
1. **컨텍스트 부담 감소** - 핵심 결정만 참조
2. **지식 보존** - 세션 종료 후에도 참조 가능
3. **협업 용이** - 다른 에이전트/개발자와 공유

### 문서 유형별 템플릿

| 유형 | 언제 사용 | 포함 내용 |
|------|----------|-----------|
| **ADR** | 아키텍처 결정 시 | 컨텍스트, 결정, 이유, 결과 |
| **CONTEXT** | 세션 중간 저장 | 논의 요약, 결정 사항, TODO |
| **SUMMARY** | 작업 완료 시 | 무엇을 했는지, 변경 파일 목록 |

---

**마지막 업데이트**: 2026-01-20
**관리자**: Documenter 에이전트
