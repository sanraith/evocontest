```mermaid
graph BT

A --> B
A --> D
A --> C
    C --> D
        D --> G
        D --> H
    C --> E
    C --> F

```

```mermaid
graph BT

A --> B(BB)
    B --> D(DDD)
    B --> E(EEE)
        E --> F(FFFF)
        E --> A
A --> C(CC)

```

DDD FFFF DDD FFFF DDD FFFF DDD FFFF DDD FFFF DDD FFFF DDD FFFF DDD FFFF DDD FFFF A CC CC CC CC CC CC CC CC CC

((DDD (FFFF A)) CC)
((DDD EEE) CC)
(BB CC)
(A)

A(BB CC)
BB(DDD EEE)
EEE(FFFF A)

A(BB CC). BB(DDD EEE). DDD FFFF DDD FFFF DDD FFFF DDD FFFF DDD FFFF DDD FFFF DDD FFFF DDD FFFF DDD FFFF A CC CC CC CC CC CC CC CC CC. EEE(FFFF A).
