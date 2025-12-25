namespace UnitSimulator;

public class WinConditionEvaluator
{
    public void Evaluate(GameSession session)
    {
        if (session.Result != GameResult.InProgress)
        {
            return;
        }

        var friendlyKing = session.GetKingTower(UnitFaction.Friendly);
        var enemyKing = session.GetKingTower(UnitFaction.Enemy);

        bool friendlyKingDestroyed = friendlyKing?.IsDestroyed == true;
        bool enemyKingDestroyed = enemyKing?.IsDestroyed == true;

        if (friendlyKingDestroyed || enemyKingDestroyed)
        {
            if (friendlyKingDestroyed && enemyKingDestroyed)
            {
                session.Result = GameResult.Draw;
            }
            else if (enemyKingDestroyed)
            {
                session.Result = GameResult.FriendlyWin;
            }
            else
            {
                session.Result = GameResult.EnemyWin;
            }

            session.WinConditionType = WinCondition.KingDestroyed;
            return;
        }

        if (session.ElapsedTime < session.RegularTime)
        {
            return;
        }

        if (!session.IsOvertime)
        {
            if (session.FriendlyCrowns != session.EnemyCrowns)
            {
                SetWinnerByCrowns(session, WinCondition.MoreCrownCount);
                return;
            }

            session.IsOvertime = true;
            return;
        }

        if (session.FriendlyCrowns != session.EnemyCrowns)
        {
            SetWinnerByCrowns(session, WinCondition.TieBreaker);
            return;
        }

        if (session.ElapsedTime < session.MaxGameTime)
        {
            return;
        }

        float friendlyRatio = session.GetTotalTowerHPRatio(UnitFaction.Friendly);
        float enemyRatio = session.GetTotalTowerHPRatio(UnitFaction.Enemy);

        if (Math.Abs(friendlyRatio - enemyRatio) < 0.0001f)
        {
            session.Result = GameResult.Draw;
            session.WinConditionType = WinCondition.MoreTowerDamage;
            return;
        }

        session.Result = friendlyRatio > enemyRatio ? GameResult.FriendlyWin : GameResult.EnemyWin;
        session.WinConditionType = WinCondition.MoreTowerDamage;
    }

    private static void SetWinnerByCrowns(GameSession session, WinCondition condition)
    {
        session.Result = session.FriendlyCrowns > session.EnemyCrowns
            ? GameResult.FriendlyWin
            : GameResult.EnemyWin;
        session.WinConditionType = condition;
    }
}
