# Assets

Local, **non-versioned**, structured asset library consumed by `Nexus.Engine`
at render time. Audio files and final renders are **not** committed to git
(see `.gitignore`).

Expected layout:

```
Assets/
├── Music/
│   ├── Finance/      # Tension / Corporate (royalty-free or originally produced)
│   ├── TechAI/       # Synthwave
│   └── Legal/        # Dark Ambient
├── LUTs/             # `.cube` colour-grading lookup tables (anti-reused-content)
└── Fonts/            # Bold display fonts for `.ass` subtitles (Anton, Bebas Neue, …)
```

Every audio track MUST have a sibling `*.license.txt` documenting the legal
basis for use. The engine refuses to render with a track that is missing this
file.
