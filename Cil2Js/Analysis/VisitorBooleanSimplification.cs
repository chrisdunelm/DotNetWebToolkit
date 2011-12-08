using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cil2Js.Ast;
using Mono.Cecil;

namespace Cil2Js.Analysis {
    public class VisitorBooleanSimplification : AstRecursiveVisitor {

        // See http://www.d.umn.edu/~snorr/ece1315f2/BOOLLAWP.htm for all simplifications

        public static ICode V(MethodDefinition method, ICode c) {
            var v = new VisitorBooleanSimplification(method);
            return v.Visit(c);
        }

        public VisitorBooleanSimplification(MethodDefinition method) {
            this.typeSystem = method.Module.TypeSystem;
        }

        private TypeSystem typeSystem;

        // A + B is A || B
        // AB is A && B

        protected override ICode VisitUnary(ExprUnary e) {
            var eg = Expr.ExprGen(this.typeSystem);
            var op = e.Op;
            var expr = (Expr)this.Visit(e.Expr);

            // Double complement law
            if (op == UnaryOp.Not) {
                if (expr.ExprType == Expr.NodeType.Unary) {
                    var eUn = (ExprUnary)expr;
                    if (eUn.Op == UnaryOp.Not) {
                        return eUn.Expr;
                    }
                }
            }

            //// DeMorgan's laws
            //if (op == UnaryOp.Not && expr.ExprType == Expr.NodeType.Binary) {
            //    var eBin = (ExprBinary)expr;
            //    // !(AB) => !A + !B
            //    if (eBin.Op == BinaryOp.And) {
            //        return eg.Or(eg.NotAutoSimplify(eBin.Left), eg.NotAutoSimplify(eBin.Right));
            //    }
            //    // !(A + B) => (!A)(!B)
            //    if (eBin.Op == BinaryOp.Or) {
            //        return eg.And(eg.NotAutoSimplify(eBin.Left), eg.NotAutoSimplify(eBin.Right));
            //    }
            //}

            if (expr != e.Expr) {
                return new ExprUnary(op, e.Type, expr);
            } else {
                return e;
            }
        }

        protected override ICode VisitBinary(ExprBinary e) {
            var eg = Expr.ExprGen(this.typeSystem);
            var op = e.Op;
            var left = (Expr)this.Visit(e.Left);
            var right = (Expr)this.Visit(e.Right);

            // Idempotent laws
            if (op == BinaryOp.And || op == BinaryOp.Or) {
                // AA => A; A + A => A
                if (left.DoesEqual(right)) {
                    return left;
                }
            }

            // Complement laws
            if (op == BinaryOp.Or) {
                // A + !A => 1
                if (left.DoesEqualNot(right)) {
                    return new ExprLiteral(true, this.typeSystem.Boolean);
                }
            }
            if (op == BinaryOp.And) {
                // A(!A) => 0
                if (left.DoesEqualNot(right)) {
                    return new ExprLiteral(false, this.typeSystem.Boolean);
                }
            }

            // Identity and dominance laws
            if (op == BinaryOp.Or) {
                // A + 0 => A
                if (left.IsLiteralBoolean(false)) {
                    return right;
                }
                if (right.IsLiteralBoolean(false)) {
                    return left;
                }
                // A + 1 => 1
                if (left.IsLiteralBoolean(true) || right.IsLiteralBoolean(true)) {
                    return new ExprLiteral(true, this.typeSystem.Boolean);
                }
            }
            if (op == BinaryOp.And) {
                // A1 => A
                if (left.IsLiteralBoolean(true)) {
                    return right;
                }
                if (right.IsLiteralBoolean(true)) {
                    return left;
                }
                // A0 => 0
                if (left.IsLiteralBoolean(false) || right.IsLiteralBoolean(false)) {
                    return new ExprLiteral(false, this.typeSystem.Boolean);
                }
            }

            // Absorption laws
            if (op == BinaryOp.And) {
                // A(A + B) => A
                var ret = Tuple.Create(left, right).Perms((a, b) => {
                    if (a.ExprType == Expr.NodeType.Binary) {
                        var aBin = (ExprBinary)a;
                        if (aBin.Op == BinaryOp.Or) {
                            return Tuple.Create(aBin.Left, aBin.Right).Perms((c, d) => b.DoesEqual(c) ? b : null);
                        }
                    }
                    return null;
                });
                if (ret != null) {
                    return ret;
                }
            }
            if (op == BinaryOp.Or) {
                // A + AB => A
                var ret = Tuple.Create(left, right).Perms((a, b) => {
                    if (a.ExprType == Expr.NodeType.Binary) {
                        var aBin = (ExprBinary)a;
                        if (aBin.Op == BinaryOp.And) {
                            return Tuple.Create(aBin.Left, aBin.Right).Perms((c, d) => b.DoesEqual(c) ? b : null);
                        }
                    }
                    return null;
                });
                if (ret != null) {
                    return ret;
                }
            }

            // Simplification laws (must be before distributive laws)
            if (op == BinaryOp.And) {
                // A((!A) + B) => AB
                var ret = Tuple.Create(left, right).Perms((a, b) => {
                    if (a.ExprType == Expr.NodeType.Binary) {
                        var aBin = (ExprBinary)a;
                        if (aBin.Op == BinaryOp.Or) {
                            return Tuple.Create(aBin.Left, aBin.Right).Perms((c, d) => b.DoesEqualNot(c) ? eg.And(b, d) : null);
                        }
                    }
                    return null;
                });
                if (ret != null) {
                    return ret;
                }
            }
            if (op == BinaryOp.Or) {
                // A + (!A)B => A + B
                var ret = Tuple.Create(left, right).Perms((a, b) => {
                    if (a.ExprType == Expr.NodeType.Binary) {
                        var aBin = (ExprBinary)a;
                        if (aBin.Op == BinaryOp.And) {
                            return Tuple.Create(aBin.Left, aBin.Right).Perms((c, d) => b.DoesEqualNot(c) ? eg.Or(b, d) : null);
                        }
                    }
                    return null;
                });
                if (ret != null) {
                    return ret;
                }
            }
            if (op == BinaryOp.Or) {
                //// AB + AC + (!B)C => AB + (!B)C
                //var ret = Tuple.Create(left, right).Perms((a, b) => {
                //    if (a.ExprType == Expr.NodeType.Binary) {
                //        var aBin = (ExprBinary)a;
                //        if (aBin.Op == BinaryOp.Or) {
                //            return Tuple.Create(aBin.Left, aBin.Right, b).Perms((i, j, k) => {
                //                if (i.ExprType == Expr.NodeType.Binary && j.ExprType == Expr.NodeType.Binary && k.ExprType == Expr.NodeType.Binary) {
                //                    var iBin = (ExprBinary)i;
                //                    var jBin = (ExprBinary)j;
                //                    var kBin = (ExprBinary)k;
                //                    if (iBin.Op == BinaryOp.And && jBin.Op == BinaryOp.And && kBin.Op == BinaryOp.And) {
                //                        return Tuple.Create(iBin.Left, iBin.Right).Perms((m, n) => {
                //                            return Tuple.Create(jBin.Left, jBin.Right).Perms((o, p) => {
                //                                return Tuple.Create(kBin.Left, kBin.Right).Perms((q, r) => {
                //                                    if (m.DoesEqual(o) && n.DoesEqualNot(q) && p.DoesEqual(r)) {
                //                                        return Expr.Or(Expr.And(m, n), Expr.And((Expr)this.Visit(Expr.Not(n)), p));
                //                                    }
                //                                    return null;
                //                                });
                //                            });
                //                        });
                //                    }
                //                }
                //                return null;
                //            });
                //        }
                //    }
                //    return null;
                //});
                //if (ret != null) {
                //    return ret;
                //}
                // AB + C(A + !B) => AB + (!B)C
                // TODO... (not too important, fairly obscure logic)
                
            }

            // Distributive laws (must be after simplification laws)
            if (op == BinaryOp.And) {
                // (A + B)(A + C) => A + BC
                if (left.ExprType == Expr.NodeType.Binary && right.ExprType == Expr.NodeType.Binary) {
                    var lBin = (ExprBinary)left;
                    var rBin = (ExprBinary)right;
                    if (lBin.Op == BinaryOp.Or && rBin.Op == BinaryOp.Or) {
                        var ret = Tuple.Create(lBin.Left, lBin.Right).Perms((a, b) => {
                            return Tuple.Create(rBin.Left, rBin.Right).Perms((c, d) => {
                                return a.DoesEqual(c) ? eg.Or(a, eg.And(b, d)) : null;
                            });
                        });
                        if (ret != null) {
                            return ret;
                        }
                    }
                }
                // (A + B)!((!A)(!B)) => A + BC [DeMorganised version of above]
                var ret2 = Tuple.Create(left, right).Perms((a, b) => {
                    if (a.ExprType == Expr.NodeType.Binary && b.ExprType == Expr.NodeType.Unary) {
                        var aBin = (ExprBinary)a;
                        var bUn = (ExprUnary)b;
                        if (aBin.Op == BinaryOp.Or && bUn.Op == UnaryOp.Not && bUn.Expr.ExprType == Expr.NodeType.Binary) {
                            var bUnBin = (ExprBinary)bUn.Expr;
                            return Tuple.Create(aBin.Left, aBin.Right).Perms((m, n) => {
                                return Tuple.Create(bUnBin.Left, bUnBin.Right).Perms((o, p) => {
                                    if (m.DoesEqualNot(o)) {
                                        return eg.And(m, eg.And(n, eg.NotAutoSimplify(p)));
                                    }
                                    return null;
                                });
                            });
                        }
                    }
                    return null;
                });
                if (ret2 != null) {
                    return ret2;
                }
            }
            if (op == BinaryOp.Or) {
                // AB + AC => A(B + C)
                if (left.ExprType == Expr.NodeType.Binary && right.ExprType == Expr.NodeType.Binary) {
                    var lBin = (ExprBinary)left;
                    var rBin = (ExprBinary)right;
                    if (lBin.Op == BinaryOp.And && rBin.Op == BinaryOp.And) {
                        var ret = Tuple.Create(lBin.Left, lBin.Right).Perms((a, b) => {
                            return Tuple.Create(rBin.Left, rBin.Right).Perms((c, d) => {
                                return a.DoesEqual(c) ? eg.And(a, eg.Or(b, d)) : null;
                            });
                        });
                        if (ret != null) {
                            return ret;
                        }
                    }
                }
                // AB + !(!A + !C) => A(B + C) [DeMorganised version of above]
                var ret2 = Tuple.Create(left, right).Perms((a, b) => {
                    if (a.ExprType == Expr.NodeType.Binary && b.ExprType == Expr.NodeType.Unary) {
                        var aBin = (ExprBinary)a;
                        var bUn = (ExprUnary)b;
                        if (aBin.Op == BinaryOp.And && bUn.Op == UnaryOp.Not && bUn.Expr.ExprType == Expr.NodeType.Binary) {
                            var bUnBin = (ExprBinary)bUn.Expr;
                            return Tuple.Create(aBin.Left, aBin.Right).Perms((m, n) => {
                                return Tuple.Create(bUnBin.Left, bUnBin.Right).Perms((o, p) => {
                                    if (m.DoesEqualNot(o)) {
                                        return eg.And(m, eg.Or(n, eg.NotAutoSimplify(p)));
                                    }
                                    return null;
                                });
                            });
                        }
                    }
                    return null;
                });
                if (ret2 != null) {
                    return ret2;
                }
            }

            // DeMorgans laws
            if (op == BinaryOp.And) {
                // (!A)(!B) => !(A + B)
                if (left.ExprType == Expr.NodeType.Unary && right.ExprType == Expr.NodeType.Unary) {
                    var lUn = (ExprUnary)left;
                    var rUn = (ExprUnary)right;
                    if (lUn.Op == UnaryOp.Not && rUn.Op == UnaryOp.Not) {
                        return eg.Not(eg.Or(lUn.Expr, rUn.Expr));
                    }
                }
            }
            if (op == BinaryOp.Or) {
                // !A + !B => !(AB)
                if (left.ExprType == Expr.NodeType.Unary && right.ExprType == Expr.NodeType.Unary) {
                    var lUn = (ExprUnary)left;
                    var rUn = (ExprUnary)right;
                    if (lUn.Op == UnaryOp.Not && rUn.Op == UnaryOp.Not) {
                        return eg.Not(eg.And(lUn.Expr, rUn.Expr));
                    }
                }
            }

            if (left != e.Left || right != e.Right) {
                return new ExprBinary(op, e.Type, left, right);
            } else {
                return e;
            }
        }

    }
}
