# Unity 멀티 슈팅 게임 (PUN2 기반 2D 게임)

## ■ 개요
- Photon PUN2 네트워크 환경에서 동작하는 멀티 슈팅 게임입니다.
- 최대 4명이 같은 방에 접속해 동시에 플레이할 수 있으며,
  실시간 채팅 / 보스전 / 궁극기 컷인 연출까지 포함된 프로젝트입니다.
- **Unity / C# / Photon PUN2 기반으로 제작한 개인 프로젝트입니다.**

## ■ 개발 환경
- 언어: C#
- 개발 도구: Unity 2025, Visual Studio 2022
- 네트워크 : Photon PUN2

## ■ 시연 영상
- [▶ Multi Shooting 시연 영상](https://youtu.be/Ch-EU6h1Ru4)

---

## ■ 주요 구현 기능

### 1. 네트워크 시스템 (Photon PUN2)
- 방 생성 / 참가 기능 구현
- PhotonNetwork.Instantiate를 활용한 플레이어, 총알, 보스 오브젝트 네트워크 동기화
- RPC 호출로 보스 HP, 채팅 메시지 등 모든 클라이언트에 공유

### 2. 채팅 시스템
- TMP_InputField + ScrollRect 기반 채팅창 구현
- 메시지 입력 시 photonView.RPC로 전체 클라이언트에 전송
- 입력창이 포커스된 동안은 캐릭터 조작이 차단되도록 처리

### 3. 플레이어 컨트롤
- Unity New Input System 기반 이동 / 공격 / 궁극기 입력 처리
- 총알 발사 (쿨다운 적용, SFX 재생)
- 궁극기 충전 → 컷인 연출 → 레이저 발사 로 이어지는 궁극기 시스템 구현

### 4. 보스전 시스템
- StageProgress 게이지가 100% 도달 시 Warning UI 표시
- 보스 등장 + HP UI 활성화
- 3가지 보스 탄막 패턴 구현 (원형 탄막, 나선 탄막, 샷건 탄막)
- HP 동기화 + 피격 시 Hit Flash Shader 적용

### 5. UI 및 시각적 연출
- Cut-in 연출: 우상단 → 중앙 슬라이드 + 흔들림 효과
- Boss Hit Flash Shader: _WhiteAmount 기반 흰색 점멸
- Laser Shader: 노이즈 기반 흔들림 + 발광 효과
- Warning UI 깜빡임 및 StageProgress 게이지 애니메이션
---

## ■ 담당 파트 핵심 요약
-Photon PUN2 네트워크 시스템 (방 생성, 참가, 오브젝트 동기화, RPC 통신)

-TMP 기반 실시간 채팅 시스템

-플레이어 조작, 총알 발사, 궁극기(컷인+레이저) 시스템 구현

-StageProgress → Warning → Boss 소환 및 탄막 패턴 구현

-Shader 연출 (레이저, Hit Flash, UI Cut-in)

---
