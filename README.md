# Projekt TEG

## Spis tresci
- [Ogólna koncepcja](#ogólna-koncepcja)
- [Członkowie](#członkowie)
- [TODO](#todo)
- [Podział zadań](#podział-zadań)
- [Plan spotkań](#plan-spotkań)
- [Kamienie milowe](#kamienie-milowe)
- [Struktura git](#struktura-git)

## Ogólna koncepcja
Projekt ma polegać na stworzeniu systemu wieloagentowego obsługującego interakcje gracza z postaciami NPC na podstawie prostego demo w Unity. Postacie NPC mają mieć możliwość komunikacji zarówno z graczem jak w między sobą. Każdy z nich ma odgrywać określoną rolę oraz przekazywać graczowi tylko informacje związane z jego rolą w grze. Dodatkowo interakcje gracza z NPC mają wpływać na interakcje z pozostałymi NPC.

## Członkowie

Mateusz Sasor-Adamczyk <br/>
Błażej Bartkiewicz <br/>
Cezary Daniłowski <br/>
Michał Dębski <br/>

## TODO

<details open>
  <summary>Na 16.05.2025r.</summary>

  - [x] Wypełcić jire
  - [x] Sprawdzić czy w unity da się opdalać pythona
  - [x] Rozpoczęcie projektu w pythonie
  - [x] Podstawowe diagramy
  - [x] Stworzenie koncepcji miasta

</details>

<details open>
  <summary>20.05.2025r.</summary>

  - [x] Model w Unity
  - [x] Podstawowy agent działający w konsoli
  - [x] Stworzenie menadżera który będzie synchronizował działanie agentów
  - [x] Stworzenie API od strony Unity działającego z modelami NPC
  - [x] Stworzenie API w pythonie działającego z LLM'em
  - [x] Opracowanie formatu pdf (lub czegoś innego) dla RAG'u dla LLM'a

</details>

<details open>
  <summary>03.06.2025r.</summary>

  - [x] Stworzenie drugiego agenta oraz zaprojektowanie interakcji między nimi
  - [ ] Wywoływanie metod z poziomu LLM'a
  - [ ] Opracowanie schematu wywołania metody w Unity/przekazania aktualizacji eq
  - [ ] Multithreading dla pip'a
  - [ ] Testy agentów
  - [ ] Poprawa komunikacji pip'a z managerem LLM'ów
  - [ ] Odpalanie skryptu pythona z Unity
  - [ ] Opracowanie systemu eq

</details>

<details open>
  <summary>Next</summary>

  - [ ] Opracowanie prezentacji

</details>

## Plan spotkań

1. *12.05.2025r.* - omówienie założeń i opracowanie planu dalszego rozwoju projektu
2. *16.05.2025r.* - prezentacja dotychczasowych postępów oraz podział zadań związanych z implementacją elementów potrzebnych do demo

## Podział zadań
| Sprint | Mateusz Sasor-Adamczyk | Błażej Bartkiewicz | Cezary Daniłowski | Michał Dębski |
| :--: | -- | -- | -- | -- |
| 1 | Wypełnić jire | Rozpoczęcie projektu w pythonie | Podstawowe diagramy | Stworzenie koncepcji miasta |
| 1 | Sprawdzić czy w unity da się opdalać pythona | | |
| 2 | Stworzenie API od strony Unity | Podstawowy agent działający | Opracowanie formatu pdf (lub czegoś innego) dla RAG'u dla LLM'a | Model w Unity |
| 2 | Stworzenie API w pythonie |  | Stworzenie menadżera |  |
| 3 | Wywoływanie metod z poziomu LLM'a | Wywoływanie metod z poziomu LLM'a | Wywoływanie metod z poziomu LLM'a | Wywoływanie metod z poziomu LLM'a |
| 3 | Multithreading dla pip'a | Testy agentów | Opracowanie schematu wywołania metody w Unity/przekazania aktualizacji eq | Opracowanie systemu eq |
| 3 | Poprawa komunikacji pip'a z managerem LLM'ów | Odpalanie skryptu pythona z Unity* | Poprawa komunikacji pip'a z managerem LLM'ów | Odpalanie skryptu pythona z Unity |
| | | | | |

## Kamienie milowe
1. Rozpoczęcie projektu
   1. Diagramy
   2. Dalszy plan działania
   3. Elementy organizacyjne
      1. Podział zadań
      2. Jira
      3. Readme
   4. Szablon projektu w pythonie
   5. Plan na integracje z Unity
2. Demo
   1. Współpraca agenta z RAG
   2. Menadżer nadzorujący agenta
   3. Agent wykorzystujący przekazany mu kontekst
   4. Agent komunikujący się za pośrednictwem konsoli
3. System wieloagentowy w Unity

## Struktura git
main - środowisko produkcyjne zawierające wersje projektu gotowe do prezentacji <br/>
dev - środowisko deweloperskie zawierające wersje proejektu z zaimplementowanymi poszczególnymi funkcjonalnościami <br/>
imienne - środowiska deweloperskie w których poszczególni członkowie zespołu zajmują się implementacją poszczególnych funkcjonalności

![diagram-struktury-git](./readme_img/diagram_struktury_git.drawio.png)
