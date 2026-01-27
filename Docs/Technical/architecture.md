# Architecture

## Overview
시스템 구조 및 의존성

## Assembly Structure
```
Utility (no dependencies)
    ↑
Core, Data (depend on Utility)
    ↑
Combat, Map, Run, UI, Events (depend on Core)
    ↑
Shop, Reward (depend on Core, Data)
    ↑
Editor (development only)
```

## Key Systems
<!-- 주요 시스템 관계도 -->

## Event Flow
<!-- 이벤트 흐름 -->

## Scene Flow
<!-- 씬 전환 흐름 -->

## Notes
<!-- 추가 메모 -->
