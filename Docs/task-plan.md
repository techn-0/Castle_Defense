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
| 10 | 벽 반응 | 원거리: 매 프레임 사거리 안 벽 감지 → 정지하지 않고 이동하면서 계속 공격(사거리 벗어나거나 벽 파괴 시 해제) | 필수 | 재작성 |
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
| 32 | 로그라이크 | 벽/화염 함정 강화 추가 — 단순 수치 증가 대신 새 행동 추가: 벽은 "가시 벽"(피격 시 공격한 적에게 반격 데미지, 25G), 화염 함정은 "화상 전이"(트랩에서 벗어나도 5초간 틱뎀 지속, 25G — 최초엔 "주변 다른 적에게 전파"로 만들었다가 사용자 피드백으로 지속 화상으로 교체) | 확장(선택) | 재사용 (UpgradeManager/ShopPanelUI 패턴 확장) |
| 33 | 버그 수정 | 유인 함정에 걸린 적이 벽을 뚫고 함정으로 이동하던 문제 수정 — `Enemy.PickNextLureStep()`이 이웃 셀 선택 시 `grid.IsOccupied`를 무시하던 것(원래 닌자처럼 벽 무시가 의도였음)을 닌자 제외 전부 존중하도록 변경 + 유인 이동 중에도 매 프레임 목표 칸 점유 재검사 추가(#30과 동일한 이유) | 필수 | 재작성 (Enemy.cs) |
| 34 | 폴리시 | 화상 상태 시각 효과 — 지속 화상(#32) 중인 적 발밑에 `Assets/02_Prefebs/Flame.prefab` 이펙트 표시, 화상이 끝나면 자동 정리 | 선택 | 신규 (Enemy.cs `burnEffectPrefab`/`ApplyBurn`/`ClearBurnEffect`) |
| 35 | 배치 | 건설 모드 진입 시 화염 함정(유인형) 유인 범위 시각화 — 이미 설치된 FireTrap 전부 + 배치 전 미리보기 모두에 유인 범위 원(LineRenderer) 표시 | 필수 | 신규 (RangeIndicatorUtil.cs) + 재사용 (Player.cs 건설 범위 원 패턴) |
| 36 | 웨이브 | 페이즈 장기화 (2차 리팩터 — 최종) — 최초엔 `phaseDuration`(웨이브 목표 지속시간)과 entry별 `interval`을 조합해 간격을 역산하는 방식으로 만들었으나, 마리 수가 적을 때 역산된 간격이 몇 초씩 벌어져 "한 마리 나오고 한참 뒤 다음 한 마리"처럼 보이는 문제가 발생(사용자 피드백: "적이 끈임없이 밀고 들어와야 하는데 한 마리씩 뜸하게 나온다"). `phaseDuration`/`interval` 필드를 전부 제거하고, 대신 `spawnInterval`(몹 한 마리 스폰 후 다음 한 마리까지 고정 대기시간, 예: 0.4~0.6초) 하나로 단순화. `WaveManager.SpawnWave()`는 매 스폰마다 "남은 수량에 비례한 가중치"로 종류를 뽑는 복원 없는 가중 추첨 방식(순수 독립 확률로 뽑으면 count=1인 보스가 아예 안 나오거나 여러 번 나올 위험이 있어서 채택)으로 재작성. 웨이브 전체 길이는 이제 `entries의 count 합 × spawnInterval`로 직관적으로 결정됨. Wave1~5.asset의 count를 대폭 상향(웨이브당 18~34마리)해 지속적으로 몰려오는 물량감을 냄 | 확장(선택) | 재작성 (WaveManager.cs, WaveData.cs) |
| 37 | 로그라이크 | 웨이브 클리어 시 3택 무료 강화 시스템 — 기존 골드 상시 상점(ShopPanelUI)을 대체. `UpgradeOption`(강화 1개, id/제목/설명/maxStacks/Apply 델리게이트) + `UpgradeChoiceManager`(14종 강화 풀 보유, 웨이브 클리어마다 무작위 3개 제시) + `UpgradeChoicePanelUI`(선택 UI). `WaveManager`에 `awaitingChoice` 가드 추가(패널이 떠 있는 동안 Enter로 웨이브 조기 시작 방지). `UpgradeManager`는 `TryUnlockX`(비용 체크)에서 `UnlockX`(무비용)로 리팩터 | 확장(선택) | 신규 (UpgradeOption.cs, UpgradeChoiceManager.cs, UpgradeChoicePanelUI.cs) + 재작성 (UpgradeManager.cs, WaveManager.cs) |
| 38 | 플레이어 | 진짜 다중발사(부채꼴) — Projectile.cs를 호밍(타겟 추적) 방식에서 고정 방향 직선 이동 + 물리 트리거 충돌 판정 방식으로 전면 리팩터(Projectile.prefab에 CircleCollider2D(Trigger)+Rigidbody2D(Kinematic) 추가). Player.Fire()가 타겟 방향 기준으로 spreadAngle만큼 벌린 N개 방향에 발사. 관통(pierce)·스플래시 강화도 함께 구현 | 확장(선택) | 재작성 (Projectile.cs) + 재사용 (ExplosiveTrap.cs 스플래시 반경 순회 패턴) |
| 39 | 로그라이크 | 강화 풀 확장용 훅 — `Economy.goldMultiplier`(골드 획득 증가), `BuildManager.costMultiplier`(건설비 할인), `CastleHealth.Heal(int)`(성벽 응급 보수) 신규 추가, 3택 강화 풀에서 사용 | 확장(선택) | 신규 (Economy.cs, BuildManager.cs, CastleHealth.cs) |

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
- 원거리: 우회로가 있어도 사거리 안에 벽이 감지되면 이동을 멈추지 않고 계속 공격 — "이동 사격"(kiting) (10)
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
10. #10 원거리: 사거리 내 벽 감지 시 정지·공격 (이후 수정 — 아래 참고)
    - 재수정: "정지하고 공격"이 부자연스럽다는 피드백에 따라, 원거리는 사거리 내 벽을 감지해도 이동을 멈추지 않고 계속 걸으면서 공격하도록 변경. `Enemy.Update()`에서 `kind == Ranged` 분기를 근접/닌자/보스(사방 막힘 시에만 정지 공격, 기존 그대로)와 완전히 분리 — 원거리는 공격 타이머만 처리하고 `return` 없이 아래 일반 이동 로직(PickNext/이동)으로 그대로 이어짐. `lockedWall`을 매 프레임 `InRangedScanRadius()`로 재검사해 벽이 죽거나 사거리를 벗어나면 즉시 해제(이동하면서 멀어질 수 있으므로 한 번 잠그면 계속 유지하던 기존 방식 대신 매번 재검증 필요).

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
31. #31 상점(강화) 패널 — 개별 오브젝트를 클릭해 강화하는 방식 대신, 패널을 열어 골드로 영구 강화를 구매하는 "테크트리 해금" 방식 채택(구매 즉시 이미 설치된 것 + 앞으로 설치할 것 모두에 적용). `UpgradeManager.cs`(Economy.cs와 동일한 싱글톤 패턴, 씬 재시작 시 인스턴스 필드라 자동 초기화)가 해금 상태/비용을 들고, `ShopPanelUI.cs`(BuildPanelUI.cs와 동일하게 라벨/interactable만 갱신)가 패널 UI를 담당. 첫 강화 항목은 가시 함정 슬로우(밟으면 3초간 이동속도 50%, 비용 30G — 벽10/가시15/화염20/폭탄25 대비 가시 설치비의 2배 수준). `Enemy.cs`에 범용 상태 효과(`slowMultiplier`/`slowRemaining`, `ApplySlow()`)를 추가해 이후 다른 함정도 재사용 가능하도록 함. 씬 배선(상점 패널 GameObject, 버튼, UpgradeManager 오브젝트 배치) 필요.
32. #32 벽/화염 함정 강화 추가 — "숫자만 올리는 강화는 특색이 없다"는 피드백에 따라 새 행동을 추가하는 방식으로 설계. `UpgradeManager`에 `wallThornCost`/`WallThornUnlocked`/`TryUnlockWallThorn()`(25G)과 `fireSpreadCost`/`FireSpreadUnlocked`/`TryUnlockFireSpread()`(25G)를 같은 패턴으로 추가. 벽은 `Wall.thornDamage` 필드를 신설하고, 적이 벽을 공격하는 유일한 지점(`Enemy.cs`의 `lockedWall.TakeDamage(wallDamage)` 호출부, 근접/원거리 공용)에서 강화가 해금됐으면 공격한 적에게도 즉시 반격 데미지를 준다 — 근접은 사방이 막혔을 때만 벽을 공격하므로 미로 설계 시 적이 스스로 깎이는 효과, 원거리는 벽 근처(사거리 2.5) 상시 공격 습성 때문에 늘 punish받는 효과. 화염 함정 강화는 처음엔 "주변 다른 적에게 화상 전파"(`SpreadBurn`)로 만들었으나, 실제로 원했던 건 "트랩에서 벗어나도 화상이 유지되는 것"이라는 피드백을 받아 교체 — `Enemy.cs`에 슬로우와 대칭되는 범용 화상 상태 효과(`burnDamage`/`burnTickInterval`/`burnRemaining`, `ApplyBurn()`)를 추가하고, `FireTrap.OnTriggerStay2D`의 틱 발동 시점에 `enemy.ApplyBurn(tickDamage, tickInterval, burnLingerDuration)`을 호출해 트랩 접촉이 끝나도(`burnLingerDuration`=5초 기본) 같은 틱뎀이 독자적으로 계속 들어가게 함. `ShopPanelUI`는 버튼 2개를 더 받아 반복되는 라벨 갱신 로직을 `RefreshOne()` 헬퍼로 통합. 폭탄 강화(연쇄 폭발)는 이번 라운드에서 보류, 향후 확장 후보로 남김. 씬 배선(버튼 2개 추가, 필드 연결) 필요.
33. #33 버그 수정 — 유인 함정에 걸린 적이 벽을 뚫고 이동. 원인은 `Enemy.PickNextLureStep()`이 이웃 셀을 고를 때 `grid.HasTile()`만 확인하고 `IsOccupied()`(벽 점유)는 무시하도록 설계돼 있었던 것(닌자처럼 벽을 무시하는 게 의도였는데 모든 종류에 적용됨) — 닌자를 제외한 모든 종류가 벽 점유 칸을 후보에서 제외하도록 수정, 그리고 `Update()`의 유인 이동 분기에도 매 프레임 목표 칸 점유 재검사를 추가(#30 버그 수정과 동일한 이유 — 이동 중 새로 놓인 벽에 대응).
34. #34 화상 상태 시각 효과 — 지속 화상(#32) 중인 적의 발밑에 `Assets/02_Prefebs/Flame.prefab`을 표시. `Enemy.cs`에 `burnEffectPrefab`/`burnEffectOffset` 필드와 `burnEffectInstance` 참조를 추가, `ApplyBurn()`에서 아직 안 붙어 있으면 적 transform의 자식으로 인스턴스화(이미 불붙은 상태에서 재적용되면 지속시간만 갱신하고 이펙트는 새로 만들지 않음), `burnRemaining`이 0이 되는 시점에 `ClearBurnEffect()`로 파괴. 적이 죽어서 파괴되는 경우엔 자식으로 붙여둔 덕에 유니티가 자동으로 함께 정리. 씬 배선: 각 적 프리팹(Enemy1/Enemy2/Ninja/Boss)의 `Burn Effect Prefab` 필드에 `Flame.prefab` 연결 필요, 발 위치가 어긋나면 `Burn Effect Offset`으로 미세 조정.
35. #35 화염 함정 유인 범위 시각화 — 건설 모드(아무 배치 모드나 진입 시)에서 이미 설치된 화염 함정의 유인 범위와, 배치 전 미리보기의 유인 범위를 모두 원으로 표시하고 싶다는 요청. 원 그리기 자체는 `Player.cs`의 건설 범위 인디케이터(LineRenderer, 반투명 원)와 완전히 동일한 패턴이라 그 로직을 `RangeIndicatorUtil.cs`(신규, `CreateCircle`/`SetRadius` 정적 헬퍼)로 뽑아내 재사용. `BuildManager`에 다른 싱글톤들과 동일한 `public static BuildManager I` + `LureRangeColor` 상수를 추가해 `IsBuildMode` 여부와 공용 색상을 외부(FireTrap)에서 참조할 수 있게 함. `FireTrap.Awake()`에서 자기 자신의 `lureRadius`로 원을 만들어 자식으로 붙이고, `Update()`에서 `BuildManager.I.IsBuildMode`일 때만 활성화(건설 모드가 아니면 평소엔 숨김). 미리보기 쪽은 `BuildManager.SetMode()`에서 프리팹에 `FireTrap` 컴포넌트가 붙어 있을 때만(=화염 함정 모드일 때만, 벽/가시/폭탄 등 유인 없는 종류는 원이 안 그려짐) 같은 헬퍼로 원을 하나 더 만들어 `previewRoot`에 매달아서 커서를 따라다니게 함.

### Phase 11 — 페이즈 장기화 + 3택 로그라이크 강화 (코드 작업 완료 — 씬 배선 대기 중)
36. #36 페이즈 장기화 — `WaveData.entries`의 `interval`을 "최소 스폰 간격 하한선"으로 재해석하고, 신규 `phaseDuration` 필드(웨이브가 목표로 하는 총 지속 시간)를 기준으로 entry별 실제 간격을 `Mathf.Max(entry.interval, phaseDuration / totalCount)`로 계산. `WaveManager.SpawnWave()`를 기존의 "entry 하나씩 순차 블록 스폰"에서 entry별 개별 타이머를 두고 매 프레임 라운드로빈으로 폴링하는 방식으로 재작성해, 근접/원거리/닌자가 블록으로 몰리지 않고 자연스럽게 섞여서 오래 나오게 함. `phaseDuration`이 0(미설정)이면 `sum(count*interval)`로 자동 폴백해 안전. 웨이브 클리어 판정(`!spawning && Enemy.All.Count==0`) 로직 자체는 변경 없음. Wave1~5.asset에 phaseDuration(45/55/65/75/90초)과 상향된 count 값을 직접 반영.
37. #37 웨이브 클리어 시 3택 무료 강화 — 기존 골드 상시 상점(`ShopPanelUI`, 아무 때나 가시 슬로우/벽 가시반격/화염 화상을 살 수 있던 방식)을 완전히 대체하는 로그라이크식 "웨이브 클리어 후 3택 무료 선택" 시스템 도입. `UpgradeOption.cs`(강화 하나를 나타내는 순수 C# 클래스 — id/제목/설명/maxStacks/Apply 델리게이트/CanOffer, ScriptableObject 대신 코드로 구성한 이유는 Apply가 Player/Economy/UpgradeManager 등 다른 싱글톤을 직접 참조하는 델리게이트라 직렬화 자산의 실익이 적어서), `UpgradeChoiceManager.cs`(Economy/UpgradeManager와 동일한 싱글톤 패턴, `Awake()`에서 14종 강화 풀 구성, `ShowChoices()`가 `CanOffer()`인 옵션 중 Fisher-Yates로 셔플해 최대 3개를 골라 패널에 제시하고 `Time.timeScale=0f`), `UpgradeChoicePanelUI.cs`(ShopPanelUI/BuildPanelUI와 동일한 라벨 갱신 스타일, 옵션이 매번 바뀌므로 버튼 리스너를 매번 `RemoveAllListeners()` 후 재등록) 신규 작성. `WaveManager`에 `awaitingChoice` 가드 필드 추가 — 강화 패널이 떠 있는 동안은 `Time.timeScale=0`이어도 `Input.GetKeyDown(Return)`은 계속 감지되므로, 이 가드 없이는 패널이 뜬 채로 Enter를 눌러 웨이브가 조기 시작되는 버그가 생겨 필수로 추가함. `Player.cs`/`BuildManager.cs`의 `Update()` 가드도 `Time.timeScale == 0f` 조건을 추가해 패널이 뜬 동안 이동/공격/건설 단축키 입력이 전부 막히도록 보강(둘 다 `GameManager.IsPlaying`만으로는 안 막히는 신규 엣지케이스였음). `UpgradeManager.cs`는 비용 체크가 필요 없어져 `TryUnlockX()`(비용 소비)에서 `UnlockX()`(무비용, 플래그만 세팅)로 리팩터, `xCost` 필드 제거. `ShopPanelUI.cs`는 씬에 버튼 OnClick이 여러 곳(최소 4곳) 배선돼 있어 스크립트를 바로 삭제하면 씬이 깨지므로, 일단 새 `UnlockX()` API에 맞춰 컴파일만 유지시켜 둔 상태(실제 패널/버튼 제거 및 스크립트 삭제는 아래 "남은 씬 작업" 참고).
38. #38 진짜 다중발사(부채꼴) — 기존 `Player.projectileCount`는 필드만 있고 실제로는 호밍 투사체 여러 발이 같은 표적에 수렴하는 정도였음. `Projectile.cs`를 호밍(매 프레임 타겟 재추적) 방식에서 고정 방향 직선 이동 + 물리 트리거 충돌 판정 방식으로 전면 리팩터(`Init(Vector2 direction, ...)`로 시그니처 변경, 회전은 스폰 시 1회만 설정, `OnTriggerEnter2D`로 `Enemy` 충돌 감지). `Projectile.prefab`에 기존엔 없던 `CircleCollider2D`(Is Trigger)와 `Rigidbody2D`(Kinematic)를 YAML 직접 편집으로 추가(Enemy1.prefab에는 이미 Collider2D+Rigidbody2D가 있어 Spike/FireTrap의 기존 트리거가 정상 동작했던 것과 동일한 방식). `Player.Fire()`는 타겟 방향을 기준으로 `spreadAngle`만큼 좌우로 벌린 N개 방향을 계산해(count=1→정면, count=3→-spread/0/+spread) 각 방향으로 발사. 관통(`projectilePierce`, 파괴 대신 카운트만 감소하고 계속 직진)과 스플래시(`projectileSplash`, 명중 시 `ExplosiveTrap.cs`의 반경 순회 패턴을 참고해 주변 적에게 추가 데미지) 강화도 같은 리팩터에 포함. `maxLifetime`은 사거리 강화 누적을 고려해 `Mathf.Max(3f, attackRange/projectileSpeed*1.5f)`로 여유를 둠.
39. #39 강화 풀용 신규 훅 — `Economy.goldMultiplier`(골드 획득 시 곱연산, `waveClearBonus`/`Enemy.goldReward` 모두 `AddGold()`를 거치므로 자동 적용), `BuildManager.costMultiplier`(건설 비용에 곱연산, 하한 0.3배), `CastleHealth.Heal(int)`(성 체력 즉시 회복, `maxHp` 상한 clamp) 추가 — 전부 `UpgradeChoiceManager`의 강화 풀에서 참조.

**14종 강화 풀** (웨이브1~4 클리어 후 매번 3개 무작위 제시, 웨이브5=보스 클리어는 강화 없이 바로 승리):
공격력 +1(스택5) · 공격속도 +15%(스택5) · 사거리 +1.0(스택4) · 이동속도 +0.5(스택4) · 투사체 속도 +2(스택3) · 다중 발사(스택2, 최대 3발) · 관통 사격(1회성) · 스플래시 사격(1회성) · 골드 획득 +20%(스택3) · 건설 비용 -10%(스택3) · 성벽 응급 보수(체력 깎일 때마다 재노출) · 가시 함정 슬로우/벽 가시반격/화염 지속화상(기존 3종, 각 1회성 — 골드 상점에서 이 무료 3택으로 이전).

**남은 씬 작업** (코드는 컴파일되는 상태, 아래는 Unity 에디터에서 직접 해야 함):
1. `UpgradeChoicePanel` UI 신규 구성 — Canvas 하위에 GameOverPanel/VictoryPanel과 유사하게 기본 비활성 패널 생성, 버튼 3개(각각 제목/설명 TMP_Text 자식) → `UpgradeChoicePanelUI` 컴포넌트에 `panelRoot`/`optionButtons[3]`/`titleTexts[3]`/`descTexts[3]` 배선
2. 별도 GameObject에 `UpgradeChoiceManager` 컴포넌트 추가, `panelUI` 필드를 위 패널에 연결
3. SampleScene의 기존 `ShopPanelUI` 패널/버튼 OnClick 참조 정리(또는 오브젝트 비활성화) → 정리 후에 `ShopPanelUI.cs`/`ShopPanelUI.cs.meta` 삭제(순서 반대로 하면 MissingComponent 경고 + 깨진 OnClick 참조가 남으므로 반드시 이 순서로)
4. Wave1~5.asset의 phaseDuration/count 값은 이미 코드/에셋에 반영해뒀지만, 실제 플레이 체감 길이는 플레이테스트하며 추가 조정 권장

**향후 확장(스트레치, 이번 범위 밖)**: 강화 선택 기회가 런당 4번뿐이라 14개 풀 중 최대 12개까지만 볼 수 있음. 후속 작업으로 WaveData를 `entriesPhaseA`/`entriesPhaseB` 두 그룹으로 나눠 웨이브 중간에도 미니 3택을 끼워 넣으면(웨이브 번호 표시는 5개 그대로 유지) 선택 기회를 7~8번까지 늘릴 수 있음 — `UpgradeChoiceManager`/`UpgradeChoicePanelUI`를 그대로 재사용 가능.

## 4. 순서를 이렇게 잡은 이유
- Phase 0~1을 가장 먼저 두는 이유: `Pathfinder`가 없으면 벽/적/함정 어떤 것도 의미 있는 테스트가 안 됨. 마일스톤 B(자동 우회)가 확인되기 전까지는 이후 작업(벽 반응, 배치 UI 등)을 만들어도 검증할 방법이 없음.
- 벽 반응 분기(Phase 2)는 이동 자체(Phase 1)가 안정된 후에 얹어야 디버깅이 쉬움. 이동과 전투 로직을 동시에 건드리면 어느 쪽 버그인지 구분이 어려움.
- 배치 시스템(Phase 3)은 벽/함정 오브젝트(Wall, Spike)가 먼저 존재해야 "배치"할 대상이 생김.
- 웨이브·재화(Phase 4)는 적/배치가 이미 동작해야 의미가 있는 상위 진행 시스템. 먼저 만들면 테스트용 더미 웨이브로 다른 시스템을 검증해야 해서 이중 작업이 됨.
- UI/게임 상태(Phase 5)는 그 아래 시스템이 만든 값(골드, 웨이브 번호, 성 HP)을 표시만 하므로 항상 마지막.
- 스트레치(Phase 6)는 MVP가 완성된 뒤 남는 시간에 우선순위 순으로 붙인다. 시간이 부족하면 스킵해도 제출물에는 지장 없음.
