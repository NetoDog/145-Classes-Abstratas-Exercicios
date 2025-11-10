// ImportacaoTemplateMethod.cs
// Solução completa e funcional do Exercicio: Validador de Importação (C#)
// Como usar: dotnet new console -o ImportacaoApp
// Substitua o Program.cs pelo conteúdo deste arquivo e rode: dotnet run

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

// Tipos de suporte
public record Registro(Dictionary<string,string> Campos)
{
    public string Get(string chave) => Campos.TryGetValue(chave, out var v) ? v : string.Empty;
}

public class Relatorio
{
    public int TotalProcessados { get; set; }
    public int TotalComErro { get; set; }
    public List<string> Erros { get; } = new List<string>();
    public Dictionary<string, int> TotaisPorCategoria { get; } = new Dictionary<string,int>();

    public string ToJson() => JsonSerializer.Serialize(this, new JsonSerializerOptions{WriteIndented=true});
}

// Classe base abstrata - ORQUESTRADOR
public abstract class ImportadorBase
{
    public Relatorio Executar(string caminho)
    {
        if (string.IsNullOrEmpty(caminho)) throw new ArgumentException("caminho não pode ser vazio");

        var rel = new Relatorio();

        if (!File.Exists(caminho)) throw new FileNotFoundException("Arquivo não encontrado", caminho);

        var lines = File.ReadAllLines(caminho).Where(l => !string.IsNullOrWhiteSpace(l)).ToArray();
        if (lines.Length == 0) return rel;

        var header = lines[0].Split(',').Select(h => h.Trim()).ToArray();

        for (int i = 1; i < lines.Length; i++)
        {
            var line = lines[i];
            var cols = line.Split(',');
            var campos = new Dictionary<string,string>(StringComparer.OrdinalIgnoreCase);
            for (int c = 0; c < header.Length && c < cols.Length; c++) campos[header[c]] = cols[c].Trim();
            var registro = new Registro(campos);

            var erros = ValidarRegistro(registro);

            rel.TotalProcessados++;
            if (erros != null && erros.Count > 0)
            {
                rel.TotalComErro++;
                foreach (var e in erros) rel.Erros.Add($"Linha {i+1}: {e}");
            }
            else
            {
                AtualizarTotais(registro, rel);
            }
        }

        PosConsolidacao(rel);
        return rel;
    }

    protected abstract List<string> ValidarRegistro(Registro r);

    protected virtual void PosConsolidacao(Relatorio rel) { }

    protected virtual void AtualizarTotais(Registro r, Relatorio rel) { }
}

// Implementacao para Alunos
public class ImportacaoAlunos : ImportadorBase
{
    protected override List<string> ValidarRegistro(Registro r)
    {
        var errors = new List<string>();
        var id = r.Get("Id");
        var nome = r.Get("Nome");
        var turma = r.Get("Turma");

        if (string.IsNullOrWhiteSpace(id)) errors.Add("Id ausente");
        if (string.IsNullOrWhiteSpace(nome)) errors.Add("Nome ausente");
        if (string.IsNullOrWhiteSpace(turma)) errors.Add("Turma ausente");

        return errors;
    }

    protected override void PosConsolidacao(Relatorio rel)
    {
        base.PosConsolidacao(rel);
    }

    protected override void AtualizarTotais(Registro r, Relatorio rel)
    {
        var turma = r.Get("Turma");
        if (string.IsNullOrWhiteSpace(turma)) turma = "(sem turma)";
        if (!rel.TotaisPorCategoria.ContainsKey(turma)) rel.TotaisPorCategoria[turma] = 0;
        rel.TotaisPorCategoria[turma]++;
    }
}

// Implementacao para Produtos
public class ImportacaoProdutos : ImportadorBase
{
    protected override List<string> ValidarRegistro(Registro r)
    {
        var errors = new List<string>();
        var id = r.Get("Id");
        var nome = r.Get("Nome");
        var categoria = r.Get("Categoria");
        var precoStr = r.Get("Preco");

        if (string.IsNullOrWhiteSpace(id)) errors.Add("Id ausente");
        if (string.IsNullOrWhiteSpace(nome)) errors.Add("Nome ausente");
        if (string.IsNullOrWhiteSpace(categoria)) errors.Add("Categoria ausente");
        if (string.IsNullOrWhiteSpace(precoStr)) errors.Add("Preco ausente");
        else if (!decimal.TryParse(precoStr, out var preco)) errors.Add("Preco invalido");
        else if (preco < 0) errors.Add("Preco nao pode ser negativo");

        return errors;
    }

    protected override void AtualizarTotais(Registro r, Relatorio rel)
    {
        var categoria = r.Get("Categoria");
        if (string.IsNullOrWhiteSpace(categoria)) categoria = "(sem categoria)";
        if (!rel.TotaisPorCategoria.ContainsKey(categoria)) rel.TotaisPorCategoria[categoria] = 0;
        rel.TotaisPorCategoria[categoria]++;
    }
}

public static class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine(">>> Executando Importadores de Exemplo");

        var alunosPath = Path.Combine(Directory.GetCurrentDirectory(), "alunos.csv");
        var produtosPath = Path.Combine(Directory.GetCurrentDirectory(), "produtos.csv");

        File.WriteAllText(alunosPath, "Id,Nome,Turma\n1,Ana,101\n2,Bruno,101\n3,Carla,102\n4,,102\n5,Diego,\n");
        File.WriteAllText(produtosPath, "Id,Nome,Categoria,Preco\n1,Caneta,Papelaria,2.5\n2,Caderno,Papelaria,15.0\n3,Mouse,Eletronicos,-10\n4,Monitor,Eletronicos,500\n5,Lapis,,1.2\n");

        var importadores = new List<(string nome, ImportadorBase impl, string path)>
        {
            ("Alunos", new ImportacaoAlunos(), alunosPath),
            ("Produtos", new ImportacaoProdutos(), produtosPath)
        };

        foreach (var (nome, impl, path) in importadores)
        {
            Console.WriteLine($"\n--- Importando: {nome} (arquivo: {Path.GetFileName(path)}) ---");
            try
            {
                var rel = impl.Executar(path);
                var outPath = Path.ChangeExtension(path, $"{nome.ToLower()}.report.json");
                File.WriteAllText(outPath, rel.ToJson());
                Console.WriteLine($"Relatorio gerado: {outPath}");
                Console.WriteLine(rel.ToJson());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao importar {nome}: {ex.Message}");
            }
        }

        Console.WriteLine("\nExecucao finalizada.");
    }
}
