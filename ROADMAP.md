## 🧭 Server Libraries – Priority & Difficulty Overview

> Not a strict roadmap.  
> Goal: focus effort where it matters **now**, avoid wasted rewrites, and align with .NET strengths.

Legend:  
- **Priority**: 🔥 High / ⚠️ Medium / 💤 Low  
- **Difficulty**: 🟢 Easy / 🟡 Medium / 🔴 Hard

---

## 🔥 High Priority

### 🟢 Easy
- [x] `com.hytale.server.core.commands.command.*`  
  _Required for basic server control — almost done_
- [ ] `com.hytale.common.util`  
  _Mostly math/helpers → C# extension methods_
- [ ] `com.hytale.common.semver`  
  _Use existing lib + thin abstraction_

---

### 🟡 Medium
- [x] `com.hytale.codecs`  
  _Core dependency for storage, assets, networking — almost done_
- [x] `com.hytale.server.core.auth.*`  
  _Foundational for everything else — almost done_
- [ ] `com.hytale.logger.*`  
  _Already usable, but UX + observability improvements possible_

---

### 🔴 Hard
- [ ] `com.hytale.storage.*`  
  _Complex, codec-heavy, non-trivial persistence model_

---

## ⚠️ Medium Priority

### 🟢 Easy
- [ ] `com.hytale.math.*`  
  _Mostly covered by `System.Numerics.*`; fill gaps via extensions_

---

### 🟡 Medium
- [ ] `com.hytale.event.*`  
  _Needs a .NET-native equivalent (likely observer/event abstractions)_
- [ ] `com.hytale.common.thread`  
  _Rewrite adapted to .NET threading / async model_

---

### 🔴 Hard
- [ ] `com.hytale.server.*`  
  _Very large surface, deep coupling → needs its **own roadmap**_
- [ ] `com.hytale.builtin`  
  _Core gameplay logic; only after QUIC + Auth are stable_
- [ ] `com.hytale.assetstore.*`  
  _Codec-dependent, not urgent_
- [ ] `com.hytale.component.*`  
  _ECS system; experimental, testing-only_
- [ ] `com.hytale.common.procedurallib`  
  _Large, complex, time-consuming (weeks)_

---

## 💤 Low Priority / Deferred

### 🟢 Easy
- [ ] `com.hytale.function.*`  
  _Unclear value_
- [ ] `com.hytale.registry`  
  _Use case unclear_

---

### 🟡 Medium
- [ ] `com.hytale.common.plugin`  
  _Plugin system not needed short-term_
- [ ] `com.hytale.plugin.*`  
  _Far from current scope_

---

### 🚫 No Rewrite Needed
- [ ] `com.hypixel.fastutil`
- [ ] `com.hytale.unsafe`
- [ ] `com.hytale.sneakythrow`
- [ ] `com.hytale.metrics.*`
- [ ] `com.hytale.common.*`  
  _Ignored globally except items listed above_

---

## 🧠 Guiding Principles

- Infrastructure first: **codecs → auth → networking → core server**
- Prefer **adaptation** over blind 1:1 rewrites
- Heavy systems only once the foundation is stable
