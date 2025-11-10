🧩 145 – Classes Abstratas (Template Method)
💡 Objetivo

Aplicar o padrão Template Method em quatro contextos diferentes — Importação, Pedidos, Pagamento e Sync — garantindo que a lógica do fluxo principal fique centralizada em uma classe base, com variações confinadas a ganchos protected.

⚙️ Estrutura geral do padrão
Diagrama textual (hierarquia comum)
ClasseBase (abstract)
 ├── public Relatorio Executar(string caminho)   ← orquestrador fixo
 │     1. Abrir/validar fonte de dados
 │     2. Ler registros
 │     3. Validar entidade (gancho abstract)
 │     4. Processar entidade (gancho virtual)
 │     5. Consolidar resultado
 │     6. Pós-consolidação (gancho virtual)
 ├── protected abstract Validar(...)
 ├── protected virtual GanchoOpcional(...)  ← default seguro (no-op)
 └── protected guard-clauses no construtor base(...)


Cada variação concreta (Importador, Processador, Pagador, Sincronizador) implementa as regras específicas sem alterar o fluxo principal.

🧠 Justificativa – abstract vs virtual

Métodos abstract → representam obrigações invariáveis para todas as subclasses. Exemplo:

ValidarRegistro (Importação), ValidarPedido (Pedidos), EfetuarPagamento (Pagamento), Sincronizar (Sync).
Cada exercício exige comportamento específico para validação, por isso não há implementação padrão possível.

Métodos virtual → representam extensões opcionais com comportamento padrão seguro.
Exemplo: PosConsolidacao, AoProcessarPedido, AoTratarFalha, AoTratarErro.
Esses métodos possuem default no-op e são sobrescritos apenas quando a subclasse precisa adicionar lógica extra.

🧩 Exercícios incluídos
Exercício	Classe Base	Ganchos (abstract/virtual)	Classe Concreta sealed	Reuso com base.X()
Importação	ImportadorBase	1 abstract, 2 virtual	—	base.PosConsolidacao()
Pedidos	ProcessadorPedidosBase	1 abstract, 2 virtual	✅ ProcessadorPedidosSimples	base.AoProcessarPedido(), base.PosConsolidacao()
Pagamento	ProcessadorPagamentoBase	2 abstract, 2 virtual	✅ PagamentoFake	base.AoTratarFalha()
Sync	SincronizadorBase	2 abstract, 2 virtual	✅ SincronizadorFake	base.AoTratarErro()
🧪 Plano de Testes

Fluxo nominal: executar dotnet run --project <App> → gerar relatórios .report.json sem exceções.

Registros inválidos: arquivos CSV com erros devem ser processados com contagem correta e mensagens de erro.

Reuso: verificar no console/JSON que o comportamento padrão é mantido quando base.X() é chamado.

Encapsulamento: validar que nenhuma subclasse expõe novos membros public e que o fluxo não depende de if/switch por tipo.

Guard-clauses: o construtor das bases inclui protected validações de invariantes (ex.: if (string.IsNullOrEmpty(caminho)) throw ...).

✅ Checklist de Entrega

 Método orquestrador público com sequência fixa (sem if/switch por tipo)

 Até 3 ganchos protected (abstract obrigatórios / virtual opcionais)

 sealed nas classes concretas finais quando apropriado

 Ao menos um override chamando base.X()

 Invariantes protegidos no construtor base

 README com diagrama textual e justificativas

 Plano de testes documentado

🚀 Execução
dotnet run --project ImportacaoApp/ImportacaoApp.csproj
dotnet run --project PedidosApp/PedidosApp.csproj
dotnet run --project PagamentoApp/PagamentoApp.csproj
dotnet run --project SyncApp/SyncApp.csproj


Relatórios .report.json serão gerados automaticamente no diretório de cada aplicação.

🏁 Conclusão

O repositório demonstra o domínio do Template Method, com variação confinada, reuso via base.X() e estrutura extensível.
As melhorias futuras recomendadas envolvem garantir no máximo três ganchos por template e manter as invariantes centralizadas no construtor da base.
