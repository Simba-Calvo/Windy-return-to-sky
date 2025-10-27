using UnityEngine;
using System.Collections;
using System.Collections.Generic; // 🔹 Permite usar List<Transform>

// ==================================================================
// Script: inimigomov.cs
// Função: Faz o inimigo patrulhar por vários pontos, perseguir o jogador
//         e atacar quando estiver próximo.
// ==================================================================

public class inimigomov : MonoBehaviour
{
    // ===========================
    // 🔸 SEÇÃO DE PATRULHA
    // ===========================
    [Header("Patrulha")]
    public List<Transform> patrolPoints; // Lista de pontos que o inimigo vai seguir
    public float patrolSpeed = 2f;       // Velocidade da patrulha
    private int currentPointIndex = 0;   // Índice do ponto atual (começa em 0 = primeiro da lista)

    // ===========================
    // 🔸 SEÇÃO DE DETECÇÃO DO JOGADOR
    // ===========================
    [Header("Detecção do Jogador")]
    public Transform player;       // Referência ao transform do jogador
    public float chaseRange = 5f;  // Distância máxima para começar a perseguir
    public float chaseSpeed = 4f;  // Velocidade enquanto persegue

    // ===========================
    // 🔸 SEÇÃO DE ATAQUE
    // ===========================
    [Header("Ataque")]
    public float attackRange = 1f;         // Distância mínima para atacar
    public float attackCooldown = 1.5f;   // Tempo entre ataques
    private float lastAttackTime;          // Armazena o tempo do último ataque

    // ===========================
    // 🔸 COMPONENTES INTERNOS
    // ===========================
    private Rigidbody2D rb;   // Referência ao Rigidbody2D (para mover)
    private Animator anim;    // Referência ao Animator (para animações)

    private bool isWaiting = false;     // Impede ações durante espera
    private bool isRandomizing = false; // Impede ações durante comportamento aleatório

    // ==================================================================
    // 🔹 Start() — executa uma vez no início do jogo
    // ==================================================================
    void Start()
    {
        rb = GetComponent<Rigidbody2D>(); // Pega o Rigidbody2D do inimigo
        anim = GetComponent<Animator>();  // Pega o Animator (se tiver)
    }

    // ==================================================================
    // 🔹 Update() — executa a cada frame
    // ==================================================================
    void Update()
    {
        // Se o jogador não foi atribuído ou há menos de 2 pontos, não faz nada
        if (player == null || patrolPoints.Count < 2)
            return;

        // Calcula a distância entre o inimigo e o jogador
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // Se o jogador estiver dentro da área de perseguição, persegue
        if (distanceToPlayer <= chaseRange)
            ChasePlayer();
        else
            // Caso contrário, apenas patrulha entre os pontos
            Patrol();

        // Atualiza o parâmetro de animação (se houver Animator)
        if (anim != null)
            anim.SetFloat("Speed", Mathf.Abs(rb.linearVelocity.x));
    }

    // ==================================================================
    // 🔹 Patrol() — faz o inimigo andar pelos pontos
    // ==================================================================
    void Patrol()
    {
        // Se estiver parado ou aleatório, não faz nada
        if (isWaiting || isRandomizing) return;

        // Pega o ponto de destino atual
        Transform targetPoint = patrolPoints[currentPointIndex];

        // Calcula direção normalizada até o ponto
        Vector3 direction = (targetPoint.position - transform.position).normalized;

        // Aplica movimento no Rigidbody
        rb.linearVelocity = new Vector2(direction.x * patrolSpeed, rb.linearVelocity.y);

        // Faz o inimigo virar para o lado que está andando
        if (direction.x != 0)
            transform.localScale = new Vector3(Mathf.Sign(direction.x), 1, 1);

        // Calcula a distância até o ponto atual
        float distanceToTarget = Vector2.Distance(transform.position, targetPoint.position);

        // Se chegou bem perto do ponto, troca para o próximo
        if (distanceToTarget < 0.3f)
        {
            ChangeTargetPoint();
        }

        // Pequena chance de comportamento aleatório
        if (Random.value < 0.0015f)
        {
            StartCoroutine(RandomBehavior());
        }
    }

    // ==================================================================
    // 🔹 ChangeTargetPoint() — muda para o próximo ponto da lista
    // ==================================================================
    void ChangeTargetPoint()
    {
        // Aumenta o índice
        currentPointIndex++;

        // Se chegou ao fim da lista, volta para o início (loop)
        if (currentPointIndex >= patrolPoints.Count)
            currentPointIndex = 0;
    }

    // ==================================================================
    // 🔹 RandomBehavior() — comportamento aleatório (para ou muda direção)
    // ==================================================================
    IEnumerator RandomBehavior()
    {
        isRandomizing = true; // Evita que outro comportamento ocorra junto

        float randomChoice = Random.value;       // Gera um número entre 0 e 1
        float originalSpeed = patrolSpeed;       // Guarda a velocidade atual
        patrolSpeed = Random.Range(1.5f, 7f);    // Muda temporariamente a velocidade

        if (randomChoice < 0.5f)
        {
            // 🔸 Metade das vezes: o inimigo para por um tempo
            rb.linearVelocity = Vector2.zero; // Zera movimento
            yield return new WaitForSeconds(Random.Range(0.5f, 2f)); // Espera aleatoriamente
        }
        else
        {
            // 🔹 Outras vezes: muda de direção (vai pro próximo ponto)
            ChangeTargetPoint();
        }

        // Restaura comportamento normal
        isRandomizing = false;
    }

    // ==================================================================
    // 🔹 ChasePlayer() — faz o inimigo perseguir o jogador
    // ==================================================================
    void ChasePlayer()
    {
        // Calcula direção e distância até o jogador
        Vector3 direction = (player.position - transform.position).normalized;
        float distance = Vector2.Distance(transform.position, player.position);

        // Se o jogador ainda estiver longe, anda até ele
        if (distance > attackRange)
        {
            rb.linearVelocity = new Vector2(direction.x * chaseSpeed, rb.linearVelocity.y);
        }
        else
        {
            // Se estiver perto o suficiente, para e ataca
            rb.linearVelocity = Vector2.zero;
            Attack();
        }

        // Faz o inimigo olhar para o jogador
        if (direction.x != 0)
            transform.localScale = new Vector3(Mathf.Sign(direction.x), 1, 1);
    }

    // ==================================================================
    // 🔹 Attack() — executa ataque com cooldown
    // ==================================================================
    void Attack()
    {
        // Se já passou o tempo de espera desde o último ataque
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            // Atualiza o tempo do último ataque
            lastAttackTime = Time.time;

            // Aqui você pode adicionar animação, som, dano, etc.
            Debug.Log("💥 Inimigo atacou!");
        }
    }

    // ==================================================================
    // 🔹 OnDrawGizmosSelected() — desenha no editor (modo Scene)
    // ==================================================================
    void OnDrawGizmosSelected()
    {
        // Cor vermelha para visualização
        Gizmos.color = Color.red;

        // Desenha as esferas e linhas ligando os pontos
        for (int i = 0; i < patrolPoints.Count; i++)
        {
            if (patrolPoints[i] != null)
            {
                // Desenha uma esfera no ponto
                Gizmos.DrawSphere(patrolPoints[i].position, 0.1f);

                // Desenha linha até o próximo ponto, se existir
                if (i + 1 < patrolPoints.Count && patrolPoints[i + 1] != null)
                    Gizmos.DrawLine(patrolPoints[i].position, patrolPoints[i + 1].position);
            }
        }
    }
}
