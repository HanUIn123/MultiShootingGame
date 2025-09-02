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
- 방 생성과 참가 기능을 구현하였습니다.
- PhotonNetwork.Instantiate를 활용하여 플레이어, 총알, 보스 오브젝트를 네트워크로 동기화하였습니다.
- RPC 호출을 통해 보스 HP, 채팅 메시지 등을 모든 클라이언트에 공유하였습니다.
- ![Geometry Shader 연출](./Unity_Shorts/JoinRoom.gif)
### 2. 채팅 시스템
- TMP_InputField + ScrollRect 기반 채팅창을 구현하였습니다.
- 메시지 입력 시 photonView.RPC를 통해 전체 클라이언트에 전송되도록 하였습니다.
- 입력창이 포커스된 동안은 캐릭터 조작이 차단되도록 처리하였습니다.

### 3. 플레이어 컨트롤
- Unity New Input System 기반으로 이동 / 공격 / 궁극기 입력을 처리하였습니다.
- 총알 발사는 쿨다운을 적용하고, SFX가 재생되도록 구현하였습니다.
- 궁극기는 충전 → 컷인 연출 → 레이저 발사로 이어지는 시스템을 구현하였습니다.

### 4. 보스전 시스템
- StageProgress 게이지가 100% 도달하면 Warning UI가 표시되도록 하였습니다.
- 보스가 등장하고 HP UI가 활성화되도록 구현하였습니다.
- 3가지 보스 탄막 패턴(원형, 나선, 샷건)을 구현하였습니다.
- HP는 동기화되며, 피격 시 Hit Flash Shader가 적용되도록 처리하였습니다.

### 5. UI 및 시각적 연출
- Cut-in 연출은 우상단에서 중앙으로 슬라이드하며 흔들림 효과가 적용되도록 구현하였습니다.
- Boss Hit Flash Shader는 _WhiteAmount 값을 기반으로 흰색 점멸 효과가 발생하도록 하였습니다.
- Laser Shader는 노이즈 기반 흔들림과 발광 효과가 적용되도록 하였습니다.
- Warning UI는 깜빡임과 StageProgress 게이지 애니메이션을 통해 시각적 효과를 주었습니다.

---

## ■ 담당 파트 핵심 요약
- Photon PUN2 네트워크 시스템 (방 생성, 참가, 오브젝트 동기화, RPC 통신) 구현
- TMP 기반 실시간 채팅 시스템 구현
- 플레이어 조작, 총알 발사, 궁극기(컷인+레이저) 시스템 구현
- StageProgress → Warning → Boss 소환 및 탄막 패턴 구현
- Shader 연출 (레이저, Hit Flash, UI Cut-in) 구현

---
