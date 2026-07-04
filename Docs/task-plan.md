# Random Chess Defense — 작업 계획

## 1. 작업 표

| # | 카테고리 | 작업 | 우선순위 | 비고 (신규/재작성/재사용) |
|---|---|---|---|---|
| 1 | 그리드 | Grid + Tilemap 세팅 (walkable 타일 페인팅) + CastleMarker/SpawnPoint 오브젝트 배치 | 필수 | 신규 (Tilemap 에셋 + 마커 오브젝트) |
| 2 | 그리드 | TileGrid.cs — IsWalkable(x,y), SetOccupied(x,y), 월드↔셀 좌표 변환, CastleMarker/SpawnPoint 스캔 | 필수 | 신규 |
| 3 | 경로 | Pathfinder.cs — 성에서 BFS로 전체 타일 distToCastle 계산 | 필수 | 신규 (Map.ComputeDistances의 아이디어만 이식) |
| 4 | 경로 | 벽 배치/파괴 시 Pathfinder.Recompute() 훅 | 필수 | 신규 |
| 5 | 적 | Enemy.cs 이동 재작성 — 인접 4타일 중 distToCastle 최소로 이동 | 필수 | 재작성 |
| 6 | 적 | EnemyKind { Melee, Ranged } + 스프라이트 색 구분 | 필수 | 재사용 (Enemy.cs 필드 부분) |
| 7 | 적 | 적 HP + 피격/사망 처리 | 필수 | 신규 (기존엔 없었음) |
| 8 | 벽 | Wall.cs — 타일 위 배치, HP, 파괴 시 타일 해제 + BFS 재계산 트리거 | 필수 | 재사용 (HP 부분) |
| 9 | 벽 반응 | 근접: BFS가 자동 우회 처리. 사방이 다 막혔을 때 인접 벽 공격 (한 프레임 스캔) | 필수 | 재작성 |
| 10 | 벽 반응 | 원거리: 매 프레임 Physics2D.OverlapCircle로 사거리 안 벽 감지 → 정지·공격 | 필수 | 재작성 |
| 11 | 함정 | Spike.cs — 밟기 데미지 (OnTriggerEnter2D), 밟힌 횟수만큼 내구도 소모 | 필수 | 신규 |
| 12 | 배치 | 마우스 클릭 → 셀 좌표 변환 → 유효성 검사(walkable 여부, 성/스폰 아님, 이미 점유 아님) | 필수 | 신규 (BuildManager 재작성) |
| 13 | 배치 | 배치 모드 UI: 벽 / 스파이크 버튼 (임시로 1/2 키보드) | 필수 | 재사용 (BuildManager 모드 로직) |
| 14 | 배치 | 배치 미리보기: 커서 아래 셀에 반투명 표시 | 필수 | 신규 |
| 15 | 웨이브 | WaveManager.cs — ScriptableObject로 웨이브 구성, 3웨이브 진행 | 필수 | 재사용 (Spawner 스폰 코루틴 로직) |
| 16 | 웨이브 | 웨이브 사이 준비 시간 (5초) + "다음 웨이브 시작" 버튼 | 필수 | 신규 |
| 17 | 재화 | 골드 시스템 — 적 처치 보상, 웨이브 클리어 보너스, 배치 비용 차감 | 필수 | 신규 |
| 18 | UI | 상단 HUD: 골드 / 웨이브 번호 / 성 HP | 필수 | 신규 |
| 19 | UI | 하단 배치 버튼 (Canvas UI, 비용 표시) | 필수 | 신규 |
| 20 | 게임 상태 | 성 HP 0 → 게임오버 화면 / 마지막 웨이브 클리어 → 승리 화면 + 재시작 | 필수 | 재사용 (Castle.cs) |
| 21 | 함정 | Mimic.cs → FireTrap.cs(유인 + 지속 화염딜) / ExplosiveTrap.cs(밟으면 스플래시 폭발, 1회용, 유인 없음)로 분리 구현 (완료). FireTrap이 파괴 없이 무한히 지속딜을 넣어 너무 강력했던 문제를 발견해 Wall/Spike와 동일한 내구도(durability, 틱마다 소모) 적용 (완료) | 선택 | 스트레치 |
| 22 | 적 | 닌자 유닛 — 벽 무시하고 그대로 이동 (IsWalkable 체크 스킵) | 선택 | 스트레치 |
| 23 | 밸런싱 | 웨이브 4-5, 보스, 난이도 곡선 | 선택 | 스트레치 |
| 24 | 폴리시 | 벽/함정 HP 게이지, 파괴 이펙트, SFX | 선택 | 스트레치 |
| 25 | 제출물 | 시연 영상 3분, PDF 리포트, ZIP 빌드 | 필수 | 마지막 날 |
| 26 | 플레이어 | 로그라이크 플레이어 — WASD 자유 이동, 사거리 내 최근접 적 자동 타겟팅 + 화살 투사체 발사, 건설 범위를 플레이어 주변으로 제한 + 원형 인디케이터, 카메라 추적 (완료 — 씬/프리팹 배선까지) | 확장(선택) | 신규 (Player.cs, Projectile.cs, CameraFollow.cs) + 재사용 (BuildManager.CanPlace, Enemy.FindWallInRange 패턴) |
| 27 | 플레이어 | 골드로 플레이어 강화 (예: 투사체 3갈래 발사) | 확장(선택) | 미착수 — projectileCount/spreadAngle 필드만 훅으로 마련됨 |
| 28 | 배치 | 건축 모드 우클릭 취소 — 어떤 배치 모드든 우클릭 한 번으로 즉시 취소 | 필수 | 신규 (BuildManager.Update) |
| 29 | 배치 | 철거(Demolish) 기능 — 키보드 5로 철거 모드 진입, 클릭한 칸의 벽/함정을 환불 없이 즉시 파괴, 대상 위 커서 시 빨간 하이라이트 | 필수 | 신규 (BuildManager, BuildPanelUI) |
| 30 | 버그 수정 | 적이 이동 중(목표 칸 도착 전) 그 칸에 새로 놓인 벽을 뚫고 지나가던 문제 수정 — 매 프레임 목표 칸 점유 여부 재검사 + 벽/함정을 적이 현재 서 있는 칸에는 배치 불가 처리 | 필수 | 재작성 (Enemy.cs, BuildManager.CanPlace) |
| 31 | 로그라이크 | 강화 시스템 도입 — 상점(강화) 패널에서 골드로 영구 강화 구매, 가시 함정 강화(밟으면 3초 슬로우, 30G) 최초 적용 | 확장(선택) | 신규 (UpgradeManager.cs, ShopPanelUI.cs) + 재사용 (Economy 싱글톤 패턴, BuildPanelUI 라벨 갱신 패턴) |

## 2. 시스템 단위 그룹

**기반 — 자동으로 굴러가는 코어 (플레이어가 직접 안 건드림)**
- 그리드 `TileGrid` — 셀별 걷기 가능/성/스폰 여부, 점유 상태, 월드↔셀 좌표 변환 (1·2)
- 경로 `Pathfinder` — 성에서 BFS로 전 타일 distToCastle 계산, 벽 변화 시 재계산 (3·4)

**액터 — 씬 위 오브젝트**
- 적 `Enemy` — 인접 4칸 중 distToCastle 최소로 이동, HP·사망, 종류별 벽 반응 (5·6·7·9·10·22)
- 벽 `Wall` — 타일 위 배치, HP, 파괴 시 타일 해제 → 재계산 트리거 (8)
- 함정 `Trap` — 유일한 데미지원. 스파이크(필수)·미믹(스트레치) (11·21)
- 성 `Castle` — 적 도달 시 HP 감소 (20)

**플레이어 조작**
- 배치 `BuildManager` — 클릭→셀 변환→유효성 검사→생성, 미리보기 (12·13·14), + 플레이어 반경 게이트 (26)
- 재화 `Economy` — 처치·클리어 보상, 배치 비용 차감. 배치의 게이트 역할 (17)
- 플레이어 캐릭터 `Player` — WASD 자유 이동(그리드 무시), 매 프레임 사거리 내 최근접 `Enemy` 재탐색 후 `Projectile` 발사, 건설 범위 원형 인디케이터(LineRenderer) 소유. SPUM 리그 프리팹일 때 Idle/Move 애니메이션·좌우 반전도 Enemy.cs와 동일 패턴으로 처리 (26)
- 투사체 `Projectile` — 물리 콜라이더 없이 타겟 참조 + MoveTowards로 호밍, 매 프레임 이동 방향으로 회전(화살 비주얼이 날아가는 쪽을 보게), 타겟 소멸 시 자동 파괴 + 최대 수명 안전장치 (26)
- 카메라 `CameraFollow` — Main Camera에 부착, `Player` 위치를 SmoothDamp로 부드럽게 추적 (26)

**진행 & 표시**
- 웨이브 `WaveManager` — SO 웨이브 구성, 스폰, 준비 시간 (15·16)
- 게임 상태 — 게임오버/승리/재시작 (20)
- UI/HUD — 골드·웨이브·성 HP, 배치 버튼 (18·19)

### 핵심 루프 4가지
1. **경로 백본** (그리드→경로→적): 적은 스스로 판단하지 않고 distToCastle 내리막만 따라 걷는다.
2. **재탐색 루프** (이번 재설계의 핵심): 벽을 놓으면 타일이 점유 처리 → `Pathfinder.Recompute()` → 다음 프레임부터 적이 자동 우회. 별도 우회 로직이 필요 없어지는 것이 그래프 방식 대비 가장 큰 이득.
3. **데미지·재화 루프**: 함정이 적을 죽이면 골드 → 그 골드로 다시 벽·함정 배치.
4. **실패·진행**: 적이 성에 닿으면 게임오버, 웨이브가 적을 스폰하고 전멸하면 다음 웨이브.

### 유닛별 벽 반응 분기 (재미의 축)
- 근접: 재탐색 루프에 올라타 자동 우회, 사방이 막혔을 때만 벽을 공격 (9)
- 원거리: 우회로가 있어도 사거리 안에 벽이 감지되면 멈춰서 공격 — "조건부 공격" (10)
- 닌자: 벽 자체를 무시 (22, 스트레치)

## 3. 작업 순서

의존성상 위(기반)에서 아래(진행)로 진행하며, 중간중간 눈으로 확인 가능한 마일스톤을 둔다.

### Phase 0 — 기반 코어
1. #1 Tilemap 세팅
2. #2 TileGrid.cs
3. #3 Pathfinder.cs (BFS)
4. #4 벽 변화 시 Recompute 훅 (지금은 훅만, 실제 벽은 아직 없음)

→ **마일스톤 A**: 더미 이동 로직으로 성까지 최단 경로가 계산되는지 확인.

### Phase 1 — 적 이동 + 벽 기본
5. #5 Enemy 이동 재작성 (distToCastle 최소 방향 이동)
6. #7 적 HP + 피격/사망
7. #6 EnemyKind + 색 구분
8. #8 Wall.cs (배치, HP, 파괴 → 타일 해제 → Recompute)

→ **마일스톤 B**: 벽을 씬에 수동 배치했을 때 적이 자동으로 우회하는지 확인 (재탐색 루프 검증). 이번 프로젝트의 가장 중요한 검증 지점.

### Phase 2 — 벽 반응 분기 (완료 — Phase 1에서 Enemy.cs 작성 시 함께 구현됨)
9. #9 근접: 사방 막힘 시 인접 벽 공격
10. #10 원거리: 사거리 내 벽 감지 시 정지·공격

### Phase 3 — 함정 + 배치 시스템
11. #11 Spike.cs
12. #12 BuildManager 클릭 배치 (유효성 검사)
13. #13 배치 모드 전환 (벽/스파이크, 키보드 1/2)
14. #14 배치 미리보기

→ **마일스톤 C**: 플레이어가 직접 클릭으로 벽·함정을 배치하고, 적이 반응하는 전체 루프가 손으로 플레이 가능해짐.

### Phase 4 — 웨이브 + 재화 (코드 작업 완료 — 에디터 배선 대기 중)
15. #15 WaveManager.cs (SO 기반, 3웨이브)
16. #16 준비 시간 + 다음 웨이브 버튼
17. #17 골드 시스템 (보상/차감, 배치 비용 게이트)

### Phase 5 — UI + 게임 상태 (코드 작업 완료 — 에디터 배선 대기 중)
18. #18 상단 HUD
19. #19 하단 배치 버튼 UI
20. #20 게임오버/승리/재시작

→ **마일스톤 D**: 필수 항목(1~20) 완료 — 플레이 가능한 최소 완성 빌드(MVP).

### Phase 6 — 스트레치 (여유 있을 때만)
21. #23 밸런싱 (웨이브 4-5, 보스) (완료 — Wave4/5.asset 추가, EnemyKind.Boss 신설, WaveData.SpawnEntry에 웨이브별 hp/speed/gold/크기 오버라이드 필드 추가)
22. #24 폴리시 (HP 게이지, 이펙트, SFX) (완료 — Wall/Spike/Enemy HP 게이지, 파괴 파티클, SFX 훅. AudioClip은 Inspector에서 직접 배정 필요)
23. #21 Mimic.cs → FireTrap.cs / ExplosiveTrap.cs (완료) — 이후 FireTrap 밸런싱: 무한 지속딜이 너무 강력해 Wall/Spike와 동일한 내구도(durability) 적용, 소진 시 파괴 + 유인 중이던 적 정상 해제(완료). HUD 배치 버튼도 벽/가시 함정 2개뿐이었던 것을 FireTrap/ExplosiveTrap 버튼까지 4개로 확장(완료) — BuildPanelUI에 라벨 필드 추가, 씬에 버튼 GameObject 2개 배선
24. #22 닌자 유닛 (완료)
25. 종류별 적 프리팹 교체 (완료 — SPUM 기반 Enemy1.prefab(근접)·Ninja.prefab(닌자)·Enemy2.prefab(원거리, 유저가 직접 배선한 걸 발견해서 동일하게 보완)에 Enemy/Collider2D/Rigidbody2D 부착 + Idle/Move/Attack 애니메이션 전환(SPUM_Prefabs.PlayAnimation) 연결 + 이동 방향 좌우 반전(UnitRoot localScale.x) + 비주얼 세로 오프셋(-0.3) 적용. 보스는 아직 기존 Enemy.prefab(색 틴트) 사용 — 오프셋·기본 좌우 방향은 대략치라 에디터에서 육안 확인 필요)

### Phase 7 — 제출
25. #25 시연 영상, PDF 리포트, ZIP 빌드 (마지막 날)

### Phase 8 — 로그라이크 플레이어 확장 (완료 — 씬/프리팹 배선까지 끝난 상태)
26. #26 `Player.cs` — WASD 이동(자유 이동, 벽 무시), 매 프레임 `Enemy.All` 최근접 재탐색 자동 공격(`Enemy.FindWallInRange()`와 동일 패턴), 건설 범위 원형 인디케이터(LineRenderer, 건설 모드일 때만 표시)
    - `Projectile.cs` — 타겟 참조 + MoveTowards 호밍, 매 프레임 이동 방향으로 회전(화살이 날아가는 쪽을 보도록), 타겟이 비행 중 사라지면 안전하게 자폭, 최대 수명(3초) 안전장치
    - `CameraFollow.cs` — Main Camera에 부착, `Player`를 SmoothDamp로 추적
    - `BuildManager.CanPlace()`에 플레이어 반경 체크 한 줄 추가 + `IsBuildMode` getter 추가 — 기존 미리보기 흰/빨강 틴트가 그대로 반영되어 별도 UI 불필요
    - 씬 배선: 기존 SPUM 리그 `Player.prefab`(이미 씬에 배치돼 있던 것) 루트에 `Player` 컴포넌트 직접 부착, `Projectile.prefab` 신규 생성("PF Village Props - Arrow" 비주얼 자식으로 유저가 교체) 후 `Player.projectilePrefab`에 연결, Main Camera에 `CameraFollow` 부착 — 전부 프리팹/씬 파일 직접 편집으로 완료
    - 남은 것: #27 업그레이드 시스템(골드로 `projectileCount`/`spreadAngle` 등 구매) 미착수. 현재 `Projectile`은 타겟을 계속 추적하는 호밍 방식이라 `projectileCount > 1`을 켜도 여러 발이 같은 지점에 수렴함 — 실제 "부채꼴 발사"를 구현하려면 그때 가서 고정 방향 직선 이동으로 바꿔야 함

### Phase 9 — 배치 UX 보완 + 버그 수정 (완료)
28. #28 건축 모드 우클릭 취소 — `BuildManager.Update()`에서 모드가 `None`이 아닐 때 우클릭(`GetMouseButtonDown(1)`)을 감지하면 `SetMode(BuildMode.None)`으로 즉시 취소. 키보드 1~5/Esc와 별개로 언제나 동작.
29. #29 철거(Demolish) 기능 — `BuildMode.Demolish` 신설, 키보드 `5`로 진입. 셀에 놓인 Wall/Spike/FireTrap/ExplosiveTrap을 `ByCell` 딕셔너리로 찾아 `Destroy()`만 호출하는 방식으로 최대한 단순하게 구현(요청대로 환불 없음) — 각 구조물의 기존 `OnDestroy()`가 타일 점유 해제·경로 재계산·유인 해제를 알아서 처리하므로 철거 쪽에 별도 정리 코드가 필요 없었음. 별도 프리팹/에셋 없이 대상 오브젝트의 스프라이트 색을 빨갛게 덧칠하는 방식으로 최소한의 커서 하이라이트만 추가. `BuildPanelUI`에 `demolishButtonLabel` 필드 추가("철거" 텍스트) — 버튼 GameObject 자체는 기존 화염/폭탄 버튼처럼 씬에서 직접 배선 필요(선택 사항, 키보드 5만으로도 동작).
30. #30 버그 수정: 적이 벽을 뚫고 이동 — 원인은 `Enemy.PickNext()`가 도착 시점에만 다음 목표를 재계산해서, 적이 이미 목표로 정한 칸에 이동 도중 새 벽이 세워지면 재검사 없이 그 칸으로 계속 걸어 들어갔던 것(도착 후에야 우회가 반영됨). `Enemy.Update()`의 이동 분기에 매 프레임 `grid.IsOccupied(targetCell)` 재검사를 추가해, 이동 중 목표 칸이 점유되면 즉시 목표를 무효화하고 `PickNext()`를 다시 호출하도록 수정(닌자는 원래 벽을 무시하는 설계라 예외). 근본 원인 예방 차원에서 `BuildManager.CanPlace()`에도 적이 현재 서 있는 칸에는 벽/함정을 배치할 수 없도록 검사 추가. `Enemy.CurrentCell` 공개 프로퍼티 신설.

### Phase 10 — 로그라이크 강화 시스템 (코드 작업 완료 — 씬 배선 대기 중)
31. #31 상점(강화) 패널 — 개별 오브젝트를 클릭해 강화하는 방식 대신, 패널을 열어 골드로 영구 강화를 구매하는 "테크트리 해금" 방식 채택(구매 즉시 이미 설치된 것 + 앞으로 설치할 것 모두에 적용). `UpgradeManager.cs`(Economy.cs와 동일한 싱글톤 패턴, 씬 재시작 시 인스턴스 필드라 자동 초기화)가 해금 상태/비용을 들고, `ShopPanelUI.cs`(BuildPanelUI.cs와 동일하게 라벨/interactable만 갱신)가 패널 UI를 담당. 첫 강화 항목은 가시 함정 슬로우(밟으면 3초간 이동속도 50%, 비용 30G — 벽10/가시15/화염20/폭탄25 대비 가시 설치비의 2배 수준). `Enemy.cs`에 범용 상태 효과(`slowMultiplier`/`slowRemaining`, `ApplySlow()`)를 추가해 이후 다른 함정도 재사용 가능하도록 함. 향후 확장 후보(미구현): Wall HP 강화, FireTrap 틱뎀/체류시간 강화, ExplosiveTrap 범위/데미지 강화, 신규 함정(독/넉백/빙결). 씬 배선(상점 패널 GameObject, 버튼 2개, UpgradeManager 오브젝트 배치) 필요.

## 4. 순서를 이렇게 잡은 이유
- Phase 0~1을 가장 먼저 두는 이유: `Pathfinder`가 없으면 벽/적/함정 어떤 것도 의미 있는 테스트가 안 됨. 마일스톤 B(자동 우회)가 확인되기 전까지는 이후 작업(벽 반응, 배치 UI 등)을 만들어도 검증할 방법이 없음.
- 벽 반응 분기(Phase 2)는 이동 자체(Phase 1)가 안정된 후에 얹어야 디버깅이 쉬움. 이동과 전투 로직을 동시에 건드리면 어느 쪽 버그인지 구분이 어려움.
- 배치 시스템(Phase 3)은 벽/함정 오브젝트(Wall, Spike)가 먼저 존재해야 "배치"할 대상이 생김.
- 웨이브·재화(Phase 4)는 적/배치가 이미 동작해야 의미가 있는 상위 진행 시스템. 먼저 만들면 테스트용 더미 웨이브로 다른 시스템을 검증해야 해서 이중 작업이 됨.
- UI/게임 상태(Phase 5)는 그 아래 시스템이 만든 값(골드, 웨이브 번호, 성 HP)을 표시만 하므로 항상 마지막.
- 스트레치(Phase 6)는 MVP가 완성된 뒤 남는 시간에 우선순위 순으로 붙인다. 시간이 부족하면 스킵해도 제출물에는 지장 없음.
