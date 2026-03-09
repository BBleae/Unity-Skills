# Skill Security Audit | Skill 安全审计

This document records the focused security review for Unity-Skills skill discovery and execution.
本文记录 Unity-Skills 对 skill 发现与执行链路的重点安全审计结果。

## Audit Scope | 审计范围

- `SkillsForUnity/Editor/Skills/`
- `unity-skills/skills/`
- installer and generated skill metadata paths

## Result Summary | 结果摘要

- No prompt-injection strings or hidden “follow these secret instructions” content were found in shipped skill sources or skill docs.
- No malicious code patterns such as shell execution, hidden downloads, dynamic assembly loading, or credential exfiltration were found in the audited skill implementation files.
- The HTTP server is limited to `localhost`, includes request-size limits, and validates skill route names against path traversal characters.
- A real hardening gap was found: skill discovery previously scanned **all loaded assemblies**, which could auto-expose third-party methods decorated with `[UnitySkill]`.
- This repository now only discovers skills from the trusted `UnitySkills.Editor` assembly.

## Residual Risks | 剩余风险

- Some skills intentionally use reflection to inspect Unity components, events, and editor internals. This is part of the product design and should remain under manual review.
- Skills that modify files or assets still depend on path validation and Unity editor permissions; new skills should keep using `Validate.SafePath(...)` where applicable.

## Required Review For New Skills | 新增 Skill 必查项

- [ ] Do not add shell/process execution (`Process.Start`, `cmd.exe`, `/bin/sh`, `powershell`)
- [ ] Do not add hidden download/execution logic (`WebClient`, `HttpClient`, `UnityWebRequest`) unless the behavior is explicit, documented, and security-reviewed
- [ ] Do not auto-expose skills from external assemblies
- [ ] Validate file paths and asset paths before read/write/delete operations
- [ ] Reject prompt-like hidden instructions unrelated to the documented skill behavior

## Quick Manual Checks | 快速人工检查

```bash
rg -n "Process\.Start|cmd\.exe|/bin/sh|powershell|Assembly\.Load|LoadFrom|WebClient|HttpClient|UnityWebRequest|Convert\.FromBase64String" SkillsForUnity/Editor/Skills
rg -n -i "ignore previous|system prompt|prompt injection|jailbreak|忽略之前|提示词注入" .
```
