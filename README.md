# FLUXO — Drone Flight Lab (Unity)

Projeto Unity independente dos voting games, configurado para Unity **6000.3.28f1 (Unity 6.3 LTS)**, a versão LTS indicada na captura do Hub. Usa o renderizador **Built-in Render Pipeline**: não está em URP nem HDRP. Para esse primeiro protótipo leve, os materiais usam o shader Standard, com anti-aliasing, sombras suaves, névoa, iluminação fria e cores de vértice no concreto. Não requer Asset Store, plugins ou pacotes de terceiros. Os modelos, terreno, vegetação e materiais são criados proceduralmente no Editor. Consulte as [notas oficiais do Unity 6000.3.28f1](https://unity.com/releases/editor/whats-new/6000.3.28f1).

## Abrir

1. No Unity Hub, escolha **Add > Add project from disk**.
2. Selecione a pasta `DroneReplicaUnity`.
3. Abra com Unity 6000.0 ou mais recente. Na primeira abertura, aguarde o Editor compilar os scripts e gerar `Assets/Scenes/DronePrototype.unity`.
4. Se precisar recriar a cena, use **Tools > Drone Prototype > Rebuild Demo Scene**.
5. Aperte Play.

## Voo

- **WASD / setas:** aceleração e direção.
- **Clique no terreno:** redireciona o drone para aquele ponto; novos cliques substituem a trajetória imediatamente.
- **Shift:** turbo.
- **Q / E:** reduzir ou elevar altitude.
- **R:** reiniciar voo.

O controlador combina aceleração forte, velocidade com limite, arrasto no ar, freio progressivo por distância até o ponto e resposta lateral que permite curvas rápidas. Há colisão com paredes, silos e estruturas; clique no chão para apontar a próxima rota. Ajuste `Acceleration`, `Lateral Acceleration`, `Max Speed`, `Boost Multiplier` e `Brake Drag` no componente `DroneFlightController` do objeto `FPV quad · Player`.

## Build executável

Abra **File > Build Profiles**, escolha Windows ou a plataforma desejada e faça **Build**. A cena é adicionada automaticamente às cenas de build pelo gerador.

## Referência visual

O cenário parte das três imagens enviadas: fábrica abandonada em Berlim, galpão alto por dentro, silos de concreto, oficinas com janelas vazias, grafite e vegetação tomando o pátio. A cena é um bloco inicial pequeno, com um galpão principal, quatro silos e duas oficinas laterais; as formas e o grafite são procedurais e podem ser substituídos por modelos e texturas próprios.

## Limites de teste nesta sessão

O Unity Hub está instalado, mas não encontrei uma versão do Unity Editor instalada em `C:\Program Files\Unity\Hub\Editor`. A tentativa de inspecionar a janela do Hub pelo controle de interface expirou antes de mostrar a tela. Portanto, o projeto foi estruturado para abrir pelo Unity Hub, mas ainda não foi compilado nem executado em Play Mode nesta máquina.
